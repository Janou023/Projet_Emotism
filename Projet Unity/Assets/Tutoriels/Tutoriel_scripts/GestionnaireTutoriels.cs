using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GestionnaireTutoriels : MonoBehaviour
{
    private const string PrefixeCleSauvegarde = "TutorielVu_";
    private static readonly HashSet<string> tutorielsReinitialisesPourTest = new HashSet<string>();
    private static GestionnaireTutoriels instance;
    public static GestionnaireTutoriels Instance => instance != null ? instance : TrouverOuCreer();

    [SerializeField] private TutorielPopUP panneauTutoriel;
    [Tooltip("Prefab visuel du pop-up. Si vide, Resources/Tutoriels/PanneauTutoriel est utilisé.")]
    [SerializeField] private GameObject prefabPanneauTutoriel;
    [SerializeField] private ContenuTutoriel tutorielAuLancement;
    [SerializeField] private ContenuTutoriel aideCartes;
    [SerializeField] private ContenuTutoriel aideChoix;
    [SerializeField] private ContenuTutoriel aideCarnet;
    [SerializeField] private ContenuTutoriel tutorielMonde;
    [SerializeField] private ContenuTutoriel tutorielPoleEmotion;
    [SerializeField] private ContenuTutoriel tutorielDefi;
    [SerializeField] private bool reinitialiserTutorielsPourTestAuDemarrage;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ActiverSuiviDesScenes()
    {
        SceneManager.sceneLoaded -= SurSceneChargee;
        SceneManager.sceneLoaded += SurSceneChargee;
    }

    private static void SurSceneChargee(Scene scene, LoadSceneMode mode)
    {
        string nom = scene.name.ToLowerInvariant();
        if (nom == "monde")
        {
            Instance.AfficherMonde();
        }
        else if (nom.StartsWith("pole"))
        {
            Instance.AfficherTutorielPoleEmotion();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReinitialiserSessionDeTest()
    {
        tutorielsReinitialisesPourTest.Clear();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (reinitialiserTutorielsPourTestAuDemarrage)
            ResetAll();
    }

    private void Start()
    {
        if (tutorielAuLancement != null)
            AfficherTutoriel(tutorielAuLancement);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void AfficherTutoriel(ContenuTutoriel contenu)
    {
        AfficherTutorielPrive(contenu, null, true);
    }

    public bool AfficherTutoriel(ContenuTutoriel contenu, Action onClosed)
    {
        return AfficherTutorielPrive(contenu, onClosed, true);
    }

    public bool AfficherMessage(ContenuTutoriel contenu, Action onClosed = null)
    {
        return AfficherTutorielPrive(contenu, onClosed, false);
    }

    private bool AfficherTutorielPrive(ContenuTutoriel contenu, Action onClosed, bool enregistrerCommeVu)
    {
        if (contenu == null)
        {
            Debug.LogWarning("Aucun contenu de tutoriel n'a ete fourni.", this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(contenu.id))
        {
            Debug.LogError("Chaque tutoriel doit avoir un identifiant unique.", contenu);
            return false;
        }

        if (string.IsNullOrWhiteSpace(contenu.titre) || string.IsNullOrWhiteSpace(contenu.texte))
        {
            Debug.LogError("Chaque tutoriel doit avoir un titre et un texte.", contenu);
            return false;
        }

        string cleSauvegarde = PrefixeCleSauvegarde + contenu.id;
        if (enregistrerCommeVu && PlayerPrefs.GetInt(cleSauvegarde, 0) == 1)
        {
            Debug.LogWarning("Tutoriel deja enregistre comme vu : " + contenu.id, this);
            return false;
        }

        if (panneauTutoriel == null)
        {
            CreerPanneauTutoriel();
            if (panneauTutoriel == null) return false;
        }

        Debug.Log("Contenu envoye au panneau - titre: '" + contenu.titre + "', texte: '" + contenu.texte + "'.", this);
        if (!panneauTutoriel.ShowPopUp(contenu, onClosed))
        {
            return false;
        }

        if (enregistrerCommeVu)
        {
            PlayerPrefs.SetInt(cleSauvegarde, 1);
            PlayerPrefs.Save();
        }
        return true;
    }

    private static bool ReinitialiserStatutPourTest(ContenuTutoriel contenu)
    {
        if (contenu == null || string.IsNullOrWhiteSpace(contenu.id) ||
            !tutorielsReinitialisesPourTest.Add(contenu.id))
        {
            return false;
        }

        string cleSauvegarde = PrefixeCleSauvegarde + contenu.id;
        if (!PlayerPrefs.HasKey(cleSauvegarde))
        {
            return false;
        }

        PlayerPrefs.DeleteKey(cleSauvegarde);
        return true;
    }

    public void AfficherAideCartes()
    {
        ContenuTutoriel contenu = aideCartes != null ? aideCartes : Ressource("ChoixCarte");
        AfficherTutoriel(contenu);
    }

    public bool AfficherAideCartes(Action onClosed)
    {
        ContenuTutoriel contenu = aideCartes != null ? aideCartes : Ressource("ChoixCarte");
        return AfficherTutoriel(contenu, onClosed);
    }

    public void AfficherAideChoix()
    {
        AfficherTutoriel(aideChoix);
    }

    public void AfficherAideCarnet()
    {
        ContenuTutoriel contenu = aideCarnet != null ? aideCarnet : Ressource("Carnet");
        AfficherTutoriel(contenu);
    }

    public bool AfficherAideCarnet(Action onClosed)
    {
        ContenuTutoriel contenu = aideCarnet != null ? aideCarnet : Ressource("Carnet");
        return AfficherTutoriel(contenu, onClosed);
    }

    public bool AfficherDemandeCarte(Action onClosed)
    {
        ContenuTutoriel contenu = aideChoix != null ? aideChoix : Ressource("ChoixCarte");
        return AfficherTutoriel(contenu, onClosed);
    }

    public bool AfficherMonde(Action onClosed = null)
    {
        ContenuTutoriel contenu = tutorielMonde != null ? tutorielMonde : Ressource("Monde");
        return AfficherTutoriel(contenu, onClosed);
    }

    public bool AfficherTutorielPoleEmotion(Action onClosed = null)
    {
        ContenuTutoriel contenu = tutorielPoleEmotion != null
            ? tutorielPoleEmotion : Ressource("PoleEmotion");
        return AfficherTutoriel(contenu, onClosed);
    }

    public bool AfficherTutorielDefi(Action onClosed = null)
    {
        ContenuTutoriel contenu = tutorielDefi != null ? tutorielDefi : Ressource("Defi");
        return AfficherTutoriel(contenu, onClosed);
    }

    public bool AfficherTutorielNouvellePartie(Action onClosed = null)
    {
        return AfficherMessage(Ressource("IntroductionJeu"), onClosed);
    }

    public void ResetAll()
    {
        Reinitialiser(tutorielAuLancement);
        Reinitialiser(aideCartes);
        Reinitialiser(aideChoix);
        Reinitialiser(aideCarnet);
        Reinitialiser(tutorielMonde);
        Reinitialiser(tutorielPoleEmotion);
        Reinitialiser(tutorielDefi);
        Reinitialiser(Ressource("Monde"));
        Reinitialiser(Ressource("PoleEmotion"));
        Reinitialiser(Ressource("Defi"));
        Reinitialiser(Ressource("Carnet"));
        Reinitialiser(Ressource("ChoixCarte"));
        PlayerPrefs.Save();
    }

    private static void Reinitialiser(ContenuTutoriel contenu)
    {
        if (contenu == null || string.IsNullOrWhiteSpace(contenu.id)) return;
        PlayerPrefs.DeleteKey(PrefixeCleSauvegarde + contenu.id);
    }

    private static ContenuTutoriel Ressource(string nom)
    {
        return Resources.Load<ContenuTutoriel>("Tutoriels/" + nom);
    }

    private static GestionnaireTutoriels TrouverOuCreer()
    {
        instance = FindAnyObjectByType<GestionnaireTutoriels>();
        if (instance != null) return instance;

        GameObject objet = new GameObject("GestionnaireTutoriels");
        instance = objet.AddComponent<GestionnaireTutoriels>();
        instance.CreerPanneauTutoriel();
        return instance;
    }

    private void CreerPanneauTutoriel()
    {
        if (panneauTutoriel != null) return;

        GameObject prefab = prefabPanneauTutoriel != null
            ? prefabPanneauTutoriel
            : Resources.Load<GameObject>("Tutoriels/PanneauTutoriel");
        if (prefab == null)
        {
            Debug.LogError("Assigne un prefab de pop-up tutoriel ou ajoute-le dans Resources/Tutoriels/PanneauTutoriel.", this);
            return;
        }

        if (prefab.GetComponent<TutorielPopUP>() == null)
        {
            Debug.LogError("Le prefab du pop-up doit avoir TutorielPopUP sur son objet racine.", prefab);
            return;
        }

        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        GameObject canvasObjet = new GameObject("TutorielCanvas");
        canvasObjet.transform.SetParent(transform);
        Canvas canvas = canvasObjet.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler scaler = canvasObjet.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(600f, 400f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObjet.AddComponent<GraphicRaycaster>();

        GameObject instancePanneau = Instantiate(prefab, canvasObjet.transform, false);
        panneauTutoriel = instancePanneau.GetComponent<TutorielPopUP>();
        instancePanneau.SetActive(false);
    }
}
