using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Menu pause : un bouton toujours visible (en haut à droite) et la touche Échap ouvrent un menu
/// avec Continuer, Paramètres et Quitter le jeu (avec confirmation).
/// - Le jeu est gelé pendant la pause (Time.timeScale = 0) et le son est mis en pause.
/// - Le menu survit aux changements de scène : un seul exemplaire existe (comme les managers).
/// - Le clavier reste dans le menu : le focus ne peut pas partir sur l'interface du jeu derrière.
/// À placer sur la RACINE du prefab « Pause », qui contient tous les éléments de l'interface.
/// Les autres scripts peuvent lire PauseMenu.EstEnPause pour ignorer leurs commandes.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Serializable]
    class PositionRetourScene
    {
        public string sceneSource;
        public Vector3 positionDansMonde;
    }

    public static PauseMenu Instance { get; private set; }

    /// <summary>Vrai pendant la pause (et une image après, pour ne pas laisser passer la touche de fermeture).</summary>
    public static bool EstEnPause { get; private set; }

    enum Etat { Ferme, Pause, Parametres, ConfirmationQuitter }

    [Header("Bouton toujours visible")]
    [SerializeField] Button boutonPause;

    [Header("Menu pause")]
    [SerializeField] GameObject panneauPause;
    [SerializeField] Button boutonContinuer;
    [SerializeField] Button boutonParametres;
    [SerializeField] Button boutonQuitter;
    Button boutonQuitterScene;

    [Header("Paramètres (facultatif)")]
    [Tooltip("Racine du menu de paramètres, DÉSACTIVÉE dans le prefab. Vide = le bouton Paramètres est masqué.")]
    [SerializeField] GameObject menuParametres;

    [Header("Confirmation avant de quitter")]
    [Tooltip("Vide = le jeu se ferme sans confirmation.")]
    [SerializeField] GameObject panneauConfirmationQuitter;
    [SerializeField] Button boutonNonQuitter;
    [SerializeField] Button boutonOuiQuitter;

    [Header("Options")]
    [SerializeField] bool couperLeSonEnPause = true;
    [SerializeField] string sceneMenuPrincipal = "MenuPrincipal";
    [SerializeField] string sceneMonde = "Monde";
    [SerializeField] Vector3 positionRetourMonde = new Vector3(1.8f, 13.12f, 0f);
    [SerializeField] Vector3 positionRetourPolePeur = new Vector3(-1.96f, 17.04f, 0f);
    [SerializeField] PositionRetourScene[] positionsRetourParScene =
    {
        new PositionRetourScene { sceneSource = "PolePeur", positionDansMonde = new Vector3(-1.96f, 17.04f, 0f) },
        new PositionRetourScene { sceneSource = "PoleColere", positionDansMonde = new Vector3(-13.02f, 7.3f, 0f) }
    };
    [Tooltip("Scènes où la pause est indisponible (ex. écran-titre) : bouton masqué, Échap sans effet. Noms exacts.")]
    [SerializeField] string[] scenesSansPause;

    Etat etat = Etat.Ferme;
    float echelleAvantPause = 1f;
    bool disponible = true;
    GameObject cibleFocus;
    DialogueHistory historique;
    bool historiqueOuvertAvant;
    string sceneOrigineDialogue;
    Coroutine relachement;

    // ---------------------------------------------------------------- cycle de vie

    void Awake()
    {
        // Un seul menu pause dans le jeu, qui survit aux changements de scène
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Brancher(boutonPause, OuvrirPause);
        Brancher(boutonContinuer, Continuer);
        Brancher(boutonParametres, OuvrirParametres);
        Brancher(boutonQuitter, DemanderQuitter);
        CreerBoutonQuitterScene();
        Brancher(boutonNonQuitter, AnnulerQuitter);
        Brancher(boutonOuiQuitter, ConfirmerQuitter);

        // État de départ : tout est fermé
        if (panneauPause != null) panneauPause.SetActive(false);
        if (panneauConfirmationQuitter != null) panneauConfirmationQuitter.SetActive(false);
        if (menuParametres != null)
        {
            // Le menu de paramètres doit s'afficher au-dessus du menu pause et du jeu
            Canvas canvasParametres = menuParametres.GetComponent<Canvas>();
            if (canvasParametres != null) canvasParametres.sortingOrder = 300;
            menuParametres.SetActive(false);
        }

        // Le bouton Paramètres n'a pas de sens sans menu de paramètres
        if (boutonParametres != null) boutonParametres.gameObject.SetActive(menuParametres != null);

        SceneManager.sceneLoaded += SurChargementScene;
        MettreAJourScene(SceneManager.GetActiveScene());
    }

    void Start()
    {
        VerifierEventSystem();
    }

    void OnDestroy()
    {
        if (Instance != this) return;

        SceneManager.sceneLoaded -= SurChargementScene;

        // Sécurité : ne jamais laisser le jeu figé si le menu disparaît pendant une pause
        if (EstEnPause)
        {
            Time.timeScale = echelleAvantPause;
            AudioListener.pause = false;
            EstEnPause = false;
        }
        Instance = null;
    }

    void SurChargementScene(Scene scene, LoadSceneMode mode)
    {
        if (etat != Etat.Ferme) Continuer();   // on ne reste jamais en pause d'une scène à l'autre
        MettreAJourScene(scene);
        VerifierEventSystem();
    }

    void MettreAJourScene(Scene scene)
    {
        disponible = true;
        if (scenesSansPause != null)
        {
            foreach (string nom in scenesSansPause)
                if (nom == scene.name) disponible = false;
        }

        MettreAJourBoutonPause();
        if (boutonQuitterScene != null)
            boutonQuitterScene.gameObject.SetActive(EstScenePoleOuDefi(scene.name));
        historique = FindAnyObjectByType<DialogueHistory>();
    }

    static bool EstScenePoleOuDefi(string nomScene)
    {
        return !string.IsNullOrEmpty(nomScene)
            && (nomScene.StartsWith("Pole", StringComparison.OrdinalIgnoreCase)
                || nomScene.StartsWith("Defi", StringComparison.OrdinalIgnoreCase)
                || string.Equals(nomScene, "Dialogue", StringComparison.OrdinalIgnoreCase));
    }

    public void DefinirSceneOrigineDialogue(string nomScene)
    {
        sceneOrigineDialogue = nomScene;
    }

    public bool ConfigurerPositionRetourDialogue(string sceneSource, string sceneDestination)
    {
        if (TryGetPositionRetourConfiguree(sceneSource, out Vector3 position))
        {
            LoadDefi.DefinirPositionRetour(sceneDestination, position);
            return true;
        }

        if (string.Equals(sceneSource, "PolePeur", StringComparison.OrdinalIgnoreCase))
        {
            LoadDefi.DefinirPositionRetour(sceneDestination, positionRetourPolePeur);
            return true;
        }

        return false;
    }

    bool TryGetPositionRetourConfiguree(string sceneSource, out Vector3 position)
    {
        if (positionsRetourParScene != null)
        {
            foreach (PositionRetourScene config in positionsRetourParScene)
            {
                if (config == null || !string.Equals(config.sceneSource, sceneSource,
                    StringComparison.OrdinalIgnoreCase)) continue;
                position = config.positionDansMonde;
                return true;
            }
        }

        position = default;
        return false;
    }

    void CreerBoutonQuitterScene()
    {
        if (boutonQuitterScene != null || boutonContinuer == null) return;
        boutonQuitterScene = Instantiate(boutonContinuer, boutonContinuer.transform.parent);
        boutonQuitterScene.name = "Bouton_QuitterScene";
        if (boutonQuitter != null)
            boutonQuitterScene.transform.SetSiblingIndex(boutonQuitter.transform.GetSiblingIndex());
        boutonQuitterScene.onClick.RemoveAllListeners();
        TMP_Text etiquette = boutonQuitterScene.GetComponentInChildren<TMP_Text>(true);
        if (etiquette != null) etiquette.text = "Quitter la scène";
        AppliquerCouleurBouton(boutonQuitterScene, new Color(0.9098039f, 0.6392157f, 0.23921569f));
        AppliquerCouleurBouton(boutonQuitter, new Color(0.8784314f, 0.3529412f, 0.30980393f));
        Brancher(boutonQuitterScene, QuitterScenePoleOuDefi);
        boutonQuitterScene.gameObject.SetActive(false);
    }

    static void AppliquerCouleurBouton(Button bouton, Color couleur)
    {
        if (bouton == null) return;
        Graphic fond = bouton.targetGraphic;
        if (fond != null) fond.color = couleur;
    }

    void QuitterScenePoleOuDefi()
    {
        Scene sceneActuelle = SceneManager.GetActiveScene();
        if (!EstScenePoleOuDefi(sceneActuelle.name)) return;
        if (!Application.CanStreamedLevelBeLoaded(sceneMonde))
        {
            Debug.LogError("PauseMenu : scène de retour absente des scènes de build : " + sceneMonde);
            return;
        }

        if (ProgressManager.Instance != null) ProgressManager.Instance.Save();
        string sceneSource = string.Equals(sceneActuelle.name, "Dialogue", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(sceneOrigineDialogue)
            ? sceneOrigineDialogue
            : sceneActuelle.name;
        Vector3 position = positionRetourMonde;
        if (TryGetPositionRetourConfiguree(sceneSource, out Vector3 positionConfiguree))
            position = positionConfiguree;
        else if (string.Equals(sceneSource, "PolePeur", StringComparison.OrdinalIgnoreCase))
            position = positionRetourPolePeur;
        else if (!LoadDefi.TryGetReturnPosition(sceneMonde, out position))
            position = positionRetourMonde;
        LoadDefi.DefinirPositionRetour(sceneMonde, position);
        if (etat != Etat.Ferme) Continuer();
        // La sortie depuis un pôle/défi doit toujours charger le monde explicitement.
        // Le FadeManager peut encore être occupé par une transition précédente et
        // ignorer la demande, ce qui laisse croire que la scène actuelle redémarre.
        SceneManager.LoadScene(sceneMonde);
    }

    void VerifierEventSystem()
    {
        if (disponible && EventSystem.current == null)
            Debug.LogWarning("[PauseMenu] Aucun EventSystem dans la scène : les boutons de pause ne répondront pas.");
    }

    // ---------------------------------------------------------------- boucle

    void Update()
    {
        if (!disponible) return;

        if (EchapPresse()) GererEchap();

        // Le menu de paramètres s'est fermé tout seul (bouton Retour) : on revient au menu pause
        if (etat == Etat.Parametres && (menuParametres == null || !menuParametres.activeSelf))
            AllerA(Etat.Pause);

        if (etat != Etat.Ferme) MaintenirFocus();
    }

    void LateUpdate()
    {
        // État de l'historique à la fin de cette image : Échap l'a peut-être fermé pendant la suivante,
        // et cette même pression ne doit alors pas ouvrir la pause en plus.
        historiqueOuvertAvant = historique != null && historique.IsOpen;
    }

    void GererEchap()
    {
        switch (etat)
        {
            case Etat.Ferme:
            {
                // Échap sert d'abord à fermer l'historique des dialogues
                if (historiqueOuvertAvant || (historique != null && historique.IsOpen)) return;
                UIClasseur classeurOuvert = FindAnyObjectByType<UIClasseur>();
                if (classeurOuvert != null && classeurOuvert.EstOuvert
                    && !classeurOuvert.EstEnModeSelection)
                {
                    classeurOuvert.Fermer();
                    return;
                }
                OuvrirPause();
                break;
            }
            case Etat.Pause:
                Continuer();
                break;
            case Etat.Parametres:
                AllerA(Etat.Pause);
                break;
            case Etat.ConfirmationQuitter:
                AnnulerQuitter();
                break;
        }
    }

    // ---------------------------------------------------------------- actions (boutons)

    public void OuvrirPause()
    {
        if (!disponible || etat != Etat.Ferme) return;

        if (relachement != null)
        {
            StopCoroutine(relachement);
            relachement = null;
        }

        echelleAvantPause = Time.timeScale;   // on restaure la valeur d'origine, pas forcément 1
        Time.timeScale = 0f;
        if (couperLeSonEnPause) AudioListener.pause = true;
        EstEnPause = true;

        AllerA(Etat.Pause);
    }

    public void Continuer()
    {
        if (etat == Etat.Ferme) return;

        etat = Etat.Ferme;
        cibleFocus = null;
        if (panneauPause != null) panneauPause.SetActive(false);
        if (panneauConfirmationQuitter != null) panneauConfirmationQuitter.SetActive(false);
        if (menuParametres != null) menuParametres.SetActive(false);

        MettreAJourBoutonPause();

        Time.timeScale = echelleAvantPause;
        AudioListener.pause = false;

        // Plus aucun bouton sélectionné : Entrée ou Espace dans le jeu ne doivent pas « cliquer » sur un bouton du menu
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        // La pause reste active une image de plus : la touche qui vient de fermer le menu
        // (Entrée, Espace, Échap) ne doit pas agir aussi dans le jeu.
        if (relachement != null) StopCoroutine(relachement);
        relachement = StartCoroutine(RelacherPauseApresUneImage());
    }

    public void OuvrirParametres()
    {
        if (etat != Etat.Pause || menuParametres == null) return;
        AllerA(Etat.Parametres);
    }

    public void DemanderQuitter()
    {
        if (etat != Etat.Pause) return;

        if (panneauConfirmationQuitter == null)
        {
            ConfirmerQuitter();
            return;
        }
        AllerA(Etat.ConfirmationQuitter);
    }

    public void AnnulerQuitter()
    {
        if (etat != Etat.ConfirmationQuitter) return;
        AllerA(Etat.Pause);
    }

    public void ConfirmerQuitter()
    {
        // Les réglages se sauvegardent à chaque changement ; on force aussi la progression
        if (ProgressManager.Instance != null) ProgressManager.Instance.Save();

        if (etat != Etat.Ferme) Continuer();
        if (FadeManager.Instance != null)
            FadeManager.Instance.LoadSceneWithFade(sceneMenuPrincipal);
        else if (Application.CanStreamedLevelBeLoaded(sceneMenuPrincipal))
            SceneManager.LoadScene(sceneMenuPrincipal);
        else
            Debug.LogError("PauseMenu : scÃ¨ne du menu principal absente des scÃ¨nes de build : " + sceneMenuPrincipal);
    }

    // ---------------------------------------------------------------- interne

    IEnumerator RelacherPauseApresUneImage()
    {
        yield return null;
        EstEnPause = false;
        relachement = null;
    }

    void AllerA(Etat nouvelEtat)
    {
        etat = nouvelEtat;

        bool pause = nouvelEtat == Etat.Pause;
        bool confirmation = nouvelEtat == Etat.ConfirmationQuitter;

        if (panneauPause != null)
        {
            bool confirmationDansPause = panneauConfirmationQuitter != null
                && panneauConfirmationQuitter.transform.IsChildOf(panneauPause.transform);

            if (confirmationDansPause)
            {
                // Le fond assombri reste affiché, mais pendant la confirmation SEULE cette fenêtre est visible :
                // la fenêtre du menu pause est masquée pour ne pas se superposer à elle.
                panneauPause.SetActive(pause || confirmation);
                foreach (Transform enfant in panneauPause.transform)
                {
                    if (enfant.gameObject == panneauConfirmationQuitter) continue;
                    enfant.gameObject.SetActive(pause);
                }
            }
            else
            {
                panneauPause.SetActive(pause);
            }
        }

        if (panneauConfirmationQuitter != null)
            panneauConfirmationQuitter.SetActive(confirmation);
        if (menuParametres != null)
            menuParametres.SetActive(nouvelEtat == Etat.Parametres);

        MettreAJourBoutonPause();

        switch (nouvelEtat)
        {
            case Etat.Pause:
                cibleFocus = boutonContinuer != null ? boutonContinuer.gameObject : null;
                break;
            case Etat.ConfirmationQuitter:
                // Le focus est sur « Non » : un appui involontaire sur Entrée ne ferme pas le jeu
                cibleFocus = boutonNonQuitter != null ? boutonNonQuitter.gameObject : null;
                break;
            case Etat.Parametres:
                Selectable premier = menuParametres != null ? menuParametres.GetComponentInChildren<Selectable>() : null;
                cibleFocus = premier != null ? premier.gameObject : null;
                break;
            default:
                cibleFocus = null;
                break;
        }

        Focus(cibleFocus);
    }

    // Le bouton Pause n'est visible que quand le jeu tourne : pendant la pause il serait derrière les menus
    void MettreAJourBoutonPause()
    {
        if (boutonPause != null)
            boutonPause.gameObject.SetActive(disponible && etat == Etat.Ferme);
    }

    // Garde le clavier et la manette DANS le menu pause : le focus ne peut pas partir
    // vers les boutons du jeu qui se trouvent derrière le fond assombri.
    void MaintenirFocus()
    {
        EventSystem es = EventSystem.current;
        if (es == null || cibleFocus == null) return;

        GameObject selection = es.currentSelectedGameObject;
        if (selection == null || !selection.transform.IsChildOf(transform))
            es.SetSelectedGameObject(cibleFocus);
    }

    static void Focus(GameObject cible)
    {
        if (cible != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(cible);
    }

    static void Brancher(Button bouton, UnityAction action)
    {
        if (bouton == null) return;
        bouton.onClick.AddListener(action);
    }

    static bool EchapPresse()
    {
        bool presse = false;
#if ENABLE_LEGACY_INPUT_MANAGER
        presse |= Input.GetKeyDown(KeyCode.Escape);
#endif
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            presse |= Keyboard.current.escapeKey.wasPressedThisFrame;
#endif
        return presse;
    }
}
