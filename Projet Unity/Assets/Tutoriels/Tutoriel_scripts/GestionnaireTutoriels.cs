using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GestionnaireTutoriels : MonoBehaviour
{
    private const string PrefixeCleSauvegarde = "TutorielVu_";
    private static readonly HashSet<string> tutorielsReinitialisesPourTest = new HashSet<string>();
    private static GestionnaireTutoriels instance;
    public static GestionnaireTutoriels Instance => instance != null ? instance : TrouverOuCreer();

    [SerializeField] private TutorielPopUP panneauTutoriel;
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
        AfficherTutorielPrive(contenu, null);
    }

    public bool AfficherTutoriel(ContenuTutoriel contenu, Action onClosed)
    {
        return AfficherTutorielPrive(contenu, onClosed);
    }

    private bool AfficherTutorielPrive(ContenuTutoriel contenu, Action onClosed)
    {
        if (contenu == null)
        {
            Debug.LogWarning("Aucun contenu de tutoriel n'a ete fourni.", this);
            return false;
        }

        if (panneauTutoriel == null)
        {
            CreerPanneauTutoriel();
            if (panneauTutoriel == null) return false;
        }

        if (string.IsNullOrWhiteSpace(contenu.id))
        {
            Debug.LogError("Chaque tutoriel doit avoir un identifiant unique.", contenu);
            return false;
        }

        string cleSauvegarde = PrefixeCleSauvegarde + contenu.id;
        if (PlayerPrefs.GetInt(cleSauvegarde, 0) == 1)
        {
            Debug.LogWarning("Tutoriel deja enregistre comme vu : " + contenu.id, this);
            return false;
        }

        Debug.Log("Contenu envoye au panneau - titre: '" + contenu.titre + "', texte: '" + contenu.texte + "'.", this);
        if (!panneauTutoriel.ShowPopUp(contenu, onClosed))
        {
            return false;
        }

        PlayerPrefs.SetInt(cleSauvegarde, 1);
        PlayerPrefs.Save();
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

        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        GameObject canvasObjet = new GameObject("TutorielCanvas");
        canvasObjet.transform.SetParent(transform);
        Canvas canvas = canvasObjet.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        canvasObjet.AddComponent<CanvasScaler>();
        canvasObjet.AddComponent<GraphicRaycaster>();

        RectTransform panelRect = CreerRect(canvasObjet.transform, "PanneauTutoriel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 260f));
        Image panelImage = panelRect.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.12f, 0.12f, 0.12f, 0.98f);

        RectTransform titleRect = CreerRect(panelRect, "Titre",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(500f, 60f));
        titleRect.anchoredPosition = new Vector2(0f, -45f);
        TMP_Text title = titleRect.gameObject.AddComponent<TextMeshProUGUI>();
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 32f;

        RectTransform textRect = CreerRect(panelRect, "Texte",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 120f));
        textRect.anchoredPosition = new Vector2(0f, 5f);
        TMP_Text text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24f;

        Button button = CreerBouton(panelRect);

        TutorielPopUP popup = panelRect.gameObject.AddComponent<TutorielPopUP>();
        popup.Configurer(title, text);
        button.onClick.AddListener(popup.ClosePopUp);
        panelRect.gameObject.SetActive(false);
        panneauTutoriel = popup;
    }

    private static RectTransform CreerRect(Transform parent, string nom,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        GameObject objet = new GameObject(nom, typeof(RectTransform));
        objet.transform.SetParent(parent, false);
        RectTransform rect = objet.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        return rect;
    }

    private static Button CreerBouton(RectTransform parent)
    {
        RectTransform rect = CreerRect(parent, "BoutonContinuer",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(180f, 50f));
        rect.anchoredPosition = new Vector2(0f, 25f);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.25f, 0.55f, 0.8f, 1f);
        RectTransform textRect = CreerRect(rect, "Texte",
            Vector2.zero, Vector2.one, Vector2.zero);
        TMP_Text text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = "Continuer";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24f;
        return rect.gameObject.AddComponent<Button>();
    }
}
