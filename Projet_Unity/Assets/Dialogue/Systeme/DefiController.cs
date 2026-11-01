using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    /// <summary>À brancher sur ProgressionManager.Enregistrer / FeedBackManager.AfficherRetourReponseJoueur.</summary>
    public event Action<ReponseJoueur> OnReponse;

    Dictionary<int, EtapeDefi> etapes;
    Dictionary<string, Personnage> cachePersonnages;
    EtapeDefi etapeActive;
    int etapeCourante;
    bool enCours;
    bool attenteCarte;    // vrai quand on attend que le joueur place une carte

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
            Terminer();
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
                attenteCarte = true;
                dialogueManager.AfficherInvite(e.invite);
                if (classeur == null || interfaceCarte == null)
                {
                    Debug.LogError("DefiController : assigne le UIClasseur et l'interface de réponse pour afficher le carnet pendant la question.");
                    break;
                }
                interfaceCarte.OuvrirSelectionCompacte(classeur, ProposerCarte,
                    dialogueManager != null ? dialogueManager.portraitGauche : null,
                    dialogueManager != null ? dialogueManager.portraitDroit : null);
                break;
            }
            default:
            {
                if (e.fin) Terminer();
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
    public void ProposerCarte(CarteEmotion carte)
    {
        if (etapeActive == null) return;
        attenteCarte = false;

        Emotion attendue;
        if (!EtapeDefi.TryParseEmotion(etapeActive.emotionAttendue, out attendue))
            Debug.LogWarning("DefiController : émotion inconnue dans le JSON : " + etapeActive.emotionAttendue);

        ReponseJoueur r = new ReponseJoueur();
        r.carteChoisie = carte;
        r.emotionAttendue = attendue;
        r.estCorrecte = carte != null && carte.emotion == attendue;
        historique.Add(r);
        if (OnReponse != null) OnReponse(r);

        if (carte != null)
            dialogueManager.AjouterAuHistorique(dialogueManager.nomJoueur, "<i>(carte " + carte.id + ")</i>");

        dialogueManager.Masquer();
        if (etapeActive.fin) Terminer();
        else EtapeSuivante();
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
                dialogueManager != null ? dialogueManager.portraitDroit : null);
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

    void Terminer()
    {
        enCours = false;
        attenteCarte = false;
        etapeActive = null;
        dialogueManager.FinConversation();
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
