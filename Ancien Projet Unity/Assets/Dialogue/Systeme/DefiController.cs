using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Lit le JSON d'un défi et fait défiler les étapes : lignes de dialogue, choix, question « quelle émotion ? ».
/// UML : Lancer(), ProposerCarte(carte), EtapeSuivante() (privée), etapeCourante.
/// </summary>
public class DefiController : MonoBehaviour
{
    [Header("Données")]
    public TextAsset dialoguesJson;
    public Personnage[] personnages;          // Manon, Maria... (le nom doit correspondre au JSON)

    [Header("Références")]
    public DialogueManager dialogueManager;
    public UIClasseur classeur;
    public CarteReponseUI interfaceCarte;

    [Header("Résultat")]
    public List<ReponseJoueur> historique = new List<ReponseJoueur>();
    public UnityEvent onDefiTermine = new UnityEvent();
    public bool afficherAnimationVictoire = true;
    /// <summary>À brancher sur ProgressionManager.Enregistrer / FeedBackManager.AfficherRetourReponseJoueur.</summary>
    public event Action<ReponseJoueur> OnReponse;

    Dictionary<int, EtapeDefi> etapes;
    Dictionary<string, Personnage> cachePersonnages;
    EtapeDefi etapeActive;
    int etapeCourante;
    bool enCours;
    bool attenteCarte;    // vrai quand on attend que le joueur place une carte
    bool animationVictoireEnCours;

    public void Lancer()
    {
        Lancer(dialoguesJson, 0);
    }

    public void Lancer(TextAsset json, int premiereEtape)
    {
        if (enCours || DialogueManager.IsActive) return;
        if (json == null || dialogueManager == null)
        {
            Debug.LogError("DefiController : 'Dialogues Json' ou 'Dialogue Manager' manquant.");
            return;
        }

        etapes = EtapeDefi.Charger(json.text);
        PreparerPersonnages();
        historique.Clear();
        etapeCourante = premiereEtape;
        enCours = true;
        attenteCarte = false;
        JouerEtape();
    }

    void PreparerPersonnages()
    {
        cachePersonnages = new Dictionary<string, Personnage>();
        if (personnages == null) return;
        foreach (Personnage p in personnages)
        {
            if (p == null) continue;
            cachePersonnages[EtapeDefi.Normaliser(p.nom)] = p;
        }
    }

    void JouerEtape()
    {
        EtapeDefi e;
        if (!etapes.TryGetValue(etapeCourante, out e))
        {
            Debug.LogWarning("DefiController : étape introuvable : " + etapeCourante);
            Terminer(false);
            return;
        }
        etapeActive = e;

        List<Dialogue> lignes = ConstruireLignes(e);
        if (lignes.Count > 0) dialogueManager.Demarrer(lignes, ApresLignes);
        else ApresLignes();
    }

    void ApresLignes()
    {
        EtapeDefi e = etapeActive;
        switch (e.type)
        {
            case TypeQuestion.Choix:
            {
                List<string> textes = new List<string>();
                foreach (ChoixReponse c in e.choix) textes.Add(c.texte);
                dialogueManager.AfficherChoix(e.invite, textes, ChoixFait);
                break;
            }
            case TypeQuestion.CarteEmotion:
            {
                Emotion emotionAttendue;
                if (!EtapeDefi.TryParseEmotion(e.emotionAttendue, out emotionAttendue))
                {
                    Debug.LogError("DefiController : émotion inconnue dans le JSON : " + e.emotionAttendue);
                    Terminer(false);
                    return;
                }

                attenteCarte = true;
                dialogueManager.AfficherInvite(e.invite);
                if (classeur == null || interfaceCarte == null)
                {
                    Debug.LogError("DefiController : assigne le UIClasseur et l'interface de réponse pour afficher le carnet pendant la question.");
                    break;
                }
                interfaceCarte.OuvrirSelectionCompacte(classeur, ProposerCarte,
                    dialogueManager != null ? dialogueManager.portraitGauche : null,
                    dialogueManager != null ? dialogueManager.portraitDroit : null,
                    TexteIndices(e.indices), e.solution, ContinuerApresSolution);
                break;
            }
            default:
            {
                if (e.fin) Terminer(e.reussite);
                else EtapeSuivante();
                break;
            }
        }
    }

    void ChoixFait(int i)
    {
        ChoixReponse c = etapeActive.choix[i];
        dialogueManager.AjouterAuHistorique(dialogueManager.nomJoueur, c.texte);
        etapeCourante = c.suivant;
        JouerEtape();
    }

    /// <summary>Appelée quand le joueur choisit une carte dans le classeur (mode sélection).</summary>
    public bool ProposerCarte(CarteEmotion carte)
    {
        if (etapeActive == null || !attenteCarte) return false;

        Emotion attendue;
        if (!EtapeDefi.TryParseEmotion(etapeActive.emotionAttendue, out attendue))
        {
            Debug.LogError("DefiController : émotion inconnue dans le JSON : " + etapeActive.emotionAttendue);
            return false;
        }

        ReponseJoueur r = new ReponseJoueur();
        r.carteChoisie = carte;
        r.emotionAttendue = attendue;
        r.estCorrecte = carte != null && carte.emotion == attendue;
        historique.Add(r);
        if (OnReponse != null) OnReponse(r);

        if (!r.estCorrecte) return false;

        attenteCarte = false;
        if (carte != null)
            dialogueManager.AjouterAuHistorique(dialogueManager.nomJoueur, "<i>(carte " + carte.id + ")</i>");

        dialogueManager.Masquer();
        if (etapeActive.fin) Terminer();
        else EtapeSuivante();
        return true;
    }

    void ContinuerApresSolution()
    {
        if (etapeActive == null || !attenteCarte) return;

        attenteCarte = false;
        dialogueManager.Masquer();
        if (etapeActive.fin) Terminer();
        else EtapeSuivante();
    }

    static string TexteIndices(List<string> indices)
    {
        return indices != null ? string.Join("\n", indices) : "";
    }

    /// <summary>
    /// À brancher sur le bouton « Carte » (On Click).
    /// Pendant une question d'émotion : ouvre le classeur pour choisir la carte à placer.
    /// Sinon : ouvre le classeur en simple consultation.
    /// </summary>
    public void OuvrirClasseur()
    {
        if (classeur == null)
        {
            Debug.LogError("DefiController : 'Classeur' (UIClasseur) manquant.");
            return;
        }
        if (attenteCarte)
        {
            if (interfaceCarte == null)
            {
                Debug.LogError("DefiController : l'interface de confirmation des cartes est manquante.");
                return;
            }
            interfaceCarte.OuvrirSelectionCompacte(classeur, ProposerCarte,
                dialogueManager != null ? dialogueManager.portraitGauche : null,
                dialogueManager != null ? dialogueManager.portraitDroit : null,
                TexteIndices(etapeActive != null ? etapeActive.indices : null),
                etapeActive != null ? etapeActive.solution : "",
                ContinuerApresSolution);
            return;
        }

        if (interfaceCarte != null) interfaceCarte.FermerSelection();
        classeur.Ouvrir(false, null);
    }

    void EtapeSuivante()
    {
        etapeCourante++;
        JouerEtape();
    }

    void Terminer(bool afficherVictoire = true)
    {
        enCours = false;
        attenteCarte = false;
        etapeActive = null;
        dialogueManager.FinConversation();

        if (!afficherVictoire || !afficherAnimationVictoire) { NotifierFinDefi(); return; }

        VideoClip animation = Resources.Load<VideoClip>("Youpi_reussi");
        if (animation == null)
        {
            Debug.LogError("DefiController : vidéo de réussite absente de Resources/Youpi_reussi.mp4.");
            NotifierFinDefi();
            return;
        }

        JouerAnimationVictoire(animation);
    }

    void JouerAnimationVictoire(VideoClip animation)
    {
        if (animationVictoireEnCours) return;
        animationVictoireEnCours = true;

        GameObject ecran = new GameObject("AnimationVictoire");
        Canvas canvas = ecran.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = ecran.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1366f, 768f);
        scaler.matchWidthOrHeight = 0.5f;
        ecran.AddComponent<GraphicRaycaster>();

        GameObject videoObject = new GameObject(
            "Video", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        RectTransform videoRect = videoObject.transform as RectTransform;
        videoRect.SetParent(ecran.transform, false);
        videoRect.anchorMin = Vector2.zero;
        videoRect.anchorMax = Vector2.one;
        videoRect.offsetMin = Vector2.zero;
        videoRect.offsetMax = Vector2.zero;

        int width = Mathf.Clamp((int)animation.width, 1, 1920);
        int height = Mathf.Clamp((int)animation.height, 1, 1080);
        RenderTexture texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        texture.Create();
        videoObject.GetComponent<RawImage>().texture = texture;

        VideoPlayer player = ecran.AddComponent<VideoPlayer>();
        player.source = VideoSource.VideoClip;
        player.clip = animation;
        player.renderMode = VideoRenderMode.RenderTexture;
        player.targetTexture = texture;
        player.aspectRatio = VideoAspectRatio.FitInside;
        player.audioOutputMode = VideoAudioOutputMode.Direct;
        player.playOnAwake = false;
        player.isLooping = false;
        player.waitForFirstFrame = true;
        player.prepareCompleted += DemarrerAnimationVictoire;
        player.loopPointReached += TerminerAnimationVictoire;
        player.errorReceived += ErreurAnimationVictoire;
        player.Prepare();
    }

    void DemarrerAnimationVictoire(VideoPlayer player)
    {
        player.Play();
    }

    void TerminerAnimationVictoire(VideoPlayer player)
    {
        LibererAnimationVictoire(player);
    }

    void ErreurAnimationVictoire(VideoPlayer player, string message)
    {
        Debug.LogError("DefiController : lecture de la vidéo de réussite impossible : " + message);
        LibererAnimationVictoire(player);
    }

    void LibererAnimationVictoire(VideoPlayer player)
    {
        if (!animationVictoireEnCours) return;
        animationVictoireEnCours = false;
        player.prepareCompleted -= DemarrerAnimationVictoire;
        player.loopPointReached -= TerminerAnimationVictoire;
        player.errorReceived -= ErreurAnimationVictoire;

        RenderTexture texture = player.targetTexture;
        player.targetTexture = null;
        if (texture != null)
        {
            texture.Release();
            Destroy(texture);
        }

        Destroy(player.gameObject);
        NotifierFinDefi();
    }

    void NotifierFinDefi()
    {
        if (onDefiTermine != null) onDefiTermine.Invoke();
    }

    List<Dialogue> ConstruireLignes(EtapeDefi e)
    {
        List<Dialogue> liste = new List<Dialogue>();

        foreach (string t in e.pensees)
        {
            Dialogue d = new Dialogue();
            d.texte = t;
            d.pensee = true;
            liste.Add(d);
        }

        if (e.phrases.Count > 0)
        {
            Personnage p = TrouverPersonnage(e.personnage);

            // Si l'étape pose une question d'émotion, le perso montre cette émotion pendant ses phrases.
            Emotion emo = Emotion.Joie;
            bool expression = e.type == TypeQuestion.CarteEmotion && EtapeDefi.TryParseEmotion(e.emotionAttendue, out emo);

            foreach (string t in e.phrases)
            {
                Dialogue d = new Dialogue();
                d.personnage = p;
                d.texte = t;
                d.pensee = (p == null);
                d.utiliserExpression = expression;
                d.expression = emo;
                liste.Add(d);
            }
        }
        return liste;
    }

    Personnage TrouverPersonnage(string nom)
    {
        if (string.IsNullOrEmpty(nom)) return null;

        string cle = EtapeDefi.Normaliser(nom);
        Personnage p;
        if (cachePersonnages.TryGetValue(cle, out p)) return p;

        Debug.LogWarning("DefiController : pas d'asset Personnage pour '" + nom
                         + "' (à ajouter dans la liste Personnages du DefiController). Affichage avec le sprite par défaut.");
        p = new Personnage();
        p.nom = nom;
        cachePersonnages[cle] = p;
        return p;
    }
}
