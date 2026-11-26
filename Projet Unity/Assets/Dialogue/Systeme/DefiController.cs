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
    public Button boutonClasseur;
    public CardRewardPopup popupCarte;        // fenêtre « Nouvelle carte obtenue ! » (assignée par DialogueUIBootstrap)
    public EcranReussiteDefi ecranReussite;
    [Tooltip("Sous-dossier d'un dossier Resources qui contient les images citées par img_perso dans le JSON.")]
    public string dossierPortraits = "Portraits";
    [Header("Tutoriel")]
    [Tooltip("Cocher uniquement pour un vrai défi. Les dialogues de pôle émotion ne doivent pas afficher le tutoriel défi.")]
    public bool estDefi;

    [Header("Résultat")]
    public List<ReponseJoueur> historique = new List<ReponseJoueur>();
    public UnityEvent onDefiTermine = new UnityEvent();
    public bool afficherAnimationVictoire;
    public bool afficherEcranReussiteSimple;
    public string idDefi;
    /// <summary>À brancher sur ProgressionManager.Enregistrer / FeedBackManager.AfficherRetourReponseJoueur.</summary>
    public event Action<ReponseJoueur> OnReponse;

    Dictionary<int, EtapeDefi> etapes;
    Dictionary<string, Personnage> cachePersonnages;
    EtapeDefi etapeActive;
    int etapeCourante;
    bool enCours;
    bool attenteCarte;    // vrai quand on attend que le joueur place une carte
    bool animationVictoireEnCours;
    bool carteDejaDonnee; // vrai pendant qu'on traite le champ "carte_obtenue" de l'étape
    readonly Dictionary<string, Sprite> cachePortraits = new Dictionary<string, Sprite>();

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
        carteDejaDonnee = false;
        ReglerBoutonClasseur(true);
        bool tutorielAffiche = estDefi &&
            GestionnaireTutoriels.Instance.AfficherTutorielDefi(JouerEtape);
        if (!tutorielAffiche)
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

        // Étape qui donne une carte (champ "carte_obtenue" du JSON) : on la donne, on affiche
        // la fenêtre d'obtention, puis on revient ici pour continuer l'étape normalement.
        if (!string.IsNullOrEmpty(e.carteObtenue) && !carteDejaDonnee)
        {
            carteDejaDonnee = true;
            RecompenseCarte.Accorder(classeur, popupCarte, e.carteObtenue, ApresLignes);
            return;
        }
        carteDejaDonnee = false;

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
                    bool tutorielAffiche = GestionnaireTutoriels.Instance.AfficherDemandeCarte(
                        () => OuvrirQuestionCarte(e));
                    if (!tutorielAffiche)
                        OuvrirQuestionCarte(e);
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

    void OuvrirQuestionCarte(EtapeDefi etape)
    {
        if (classeur == null || interfaceCarte == null || etape == null) return;

        interfaceCarte.OuvrirSelectionCompacte(classeur, ProposerCarte,
            dialogueManager != null ? dialogueManager.portraitGauche : null,
            dialogueManager != null ? dialogueManager.portraitDroit : null,
            TexteIndices(etape.indices), etape.solution, ContinuerApresSolution);
        ReglerBoutonClasseur(false);
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
        ReglerBoutonClasseur(true);
        if (carte != null)
            dialogueManager.AjouterAuHistorique(dialogueManager.nomJoueur, "<i>(carte " + carte.id + ")</i>");

        dialogueManager.Masquer();
        bool sonCarteCorrecteJoue = SoundsManager.Instance != null
            && SoundsManager.Instance.PlayCorrectCardSound();
        if (!sonCarteCorrecteJoue && !etapeActive.fin)
            EcranReussiteDefi.JouerSonReussite();

        if (etapeActive.fin) Terminer();
        else
        {
            EtapeSuivante();
        }
        return true;
    }

    void ContinuerApresSolution()
    {
        if (etapeActive == null || !attenteCarte) return;

        attenteCarte = false;
        ReglerBoutonClasseur(true);
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
            if (classeur.EstOuvert && classeur.EstEnModeSelection) return;
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
        ReglerBoutonClasseur(true);
        etapeActive = null;
        if (afficherVictoire && estDefi && SoundsManager.Instance != null)
            SoundsManager.Instance.PlayChallengeSuccessSound();
        dialogueManager.FinConversation();

        if (afficherVictoire && !string.IsNullOrWhiteSpace(idDefi) &&
            ProgressManager.Instance != null)
            ProgressManager.Instance.CompleteChallenge(idDefi);

        if (!afficherVictoire || !afficherAnimationVictoire) { NotifierFinDefi(); return; }

        if (afficherEcranReussiteSimple)
        {
            AfficherEcranReussite();
            return;
        }

        VideoClip animation = Resources.Load<VideoClip>("Youpi_reussi");
        if (animation == null)
        {
            Debug.LogError("DefiController : vidéo de réussite absente de Resources/Youpi_reussi.mp4.");
            NotifierFinDefi();
            return;
        }

        JouerAnimationVictoire(animation);
    }

    void AfficherEcranReussite()
    {
        if (animationVictoireEnCours) return;
        animationVictoireEnCours = true;

        if (ecranReussite == null)
            ecranReussite = FindAnyObjectByType<EcranReussiteDefi>(FindObjectsInactive.Include);

        if (ecranReussite == null)
        {
            Debug.LogError("DefiController : ajoute EcranReussiteDefi dans la scène Dialogue.");
            animationVictoireEnCours = false;
            NotifierFinDefi();
            return;
        }

        ecranReussite.Afficher(() =>
        {
            animationVictoireEnCours = false;
            NotifierFinDefi();
        });
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

            // Image précise demandée par le JSON pour cette étape (champ "img_perso")
            Sprite portraitEtape = ChargerPortrait(e.imgPerso);
            if (p != null && portraitEtape != null)
                p = AvecPortrait(p, portraitEtape);

            // Si l'étape pose une question d'émotion, le perso montre cette émotion pendant ses phrases.
            Emotion emo = Emotion.Joie;
            bool expression = portraitEtape == null && e.type == TypeQuestion.CarteEmotion && EtapeDefi.TryParseEmotion(e.emotionAttendue, out emo);

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

    /// <summary>Copie du personnage qui affiche l'image demandée à la place de son portrait habituel.</summary>
    static Personnage AvecPortrait(Personnage p, Sprite sprite)
    {
        Personnage copie = new Personnage();
        copie.nom = p.nom;
        copie.cote = p.cote;
        copie.portraitNeutre = sprite;
        copie.portraits = p.portraits;
        return copie;
    }

    /// <summary>Charge l'image citée par img_perso (ex. "Julia_colere.png") depuis Resources.</summary>
    Sprite ChargerPortrait(string imgPerso)
    {
        string nom = EtapeDefi.NomSansExtension(imgPerso);
        if (nom == "") return null;

        Sprite sprite;
        if (cachePortraits.TryGetValue(nom, out sprite)) return sprite;

        string chemin = string.IsNullOrEmpty(dossierPortraits) ? nom : dossierPortraits + "/" + nom;
        Sprite[] sprites = Resources.LoadAll<Sprite>(chemin);
        foreach (Sprite candidat in sprites)
        {
            if (candidat != null &&
                (sprite == null || candidat.rect.width * candidat.rect.height > sprite.rect.width * sprite.rect.height))
            {
                sprite = candidat;
            }
        }

        if (sprite == null)
            Debug.LogWarning("DefiController : portrait introuvable « " + nom + " ». Fichier attendu : Assets/Resources/"
                             + chemin + ".png (importé en Sprite). Le portrait habituel du personnage est utilisé.");
        cachePortraits[nom] = sprite;
        return sprite;
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

    void ReglerBoutonClasseur(bool actif)
    {
        if (boutonClasseur != null)
            boutonClasseur.interactable = actif;
    }
}
