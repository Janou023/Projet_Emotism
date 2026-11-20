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
        CanvasScaler scaler = canvasObjet.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(600f, 400f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObjet.AddComponent<GraphicRaycaster>();

        RectTransform panelRect = CreerRect(canvasObjet.transform, "PanneauTutoriel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 270f));
        Image panelImage = panelRect.gameObject.AddComponent<Image>();
        ConfigurerFond(panelImage, new Color(0.88f, 0.98f, 0.99f, 0.92f));

        RectTransform titleBackground = CreerRect(panelRect, "FondTitre",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(128f, 32f));
        titleBackground.anchoredPosition = new Vector2(18f, -17f);
        ConfigurerFond(titleBackground.gameObject.AddComponent<Image>(),
            new Color(0.66f, 0.89f, 0.91f, 0.96f));
        RectTransform titleRect = CreerRect(titleBackground, "Titre",
            Vector2.zero, Vector2.one, Vector2.zero);
        titleRect.offsetMin = new Vector2(8f, 0f);
        titleRect.offsetMax = new Vector2(-8f, 0f);
        TMP_Text title = titleRect.gameObject.AddComponent<TextMeshProUGUI>();
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 16f;
        title.color = new Color(0.07f, 0.16f, 0.18f, 1f);

        RectTransform textBackground = CreerRect(panelRect, "FondDescription",
            new Vector2(0f, 0.24f), new Vector2(1f, 0.78f), new Vector2(-28f, 0f));
        textBackground.pivot = new Vector2(0.5f, 0.5f);
        ConfigurerFond(textBackground.gameObject.AddComponent<Image>(),
            new Color(0.70f, 0.95f, 0.97f, 0.94f));
        RectTransform textRect = CreerRect(textBackground, "Texte",
            Vector2.zero, Vector2.one, Vector2.zero);
        textRect.offsetMin = new Vector2(10f, 8f);
        textRect.offsetMax = new Vector2(-10f, -8f);
        TMP_Text text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 18f;
        text.fontSizeMin = 14f;
        text.fontSizeMax = 18f;
        text.enableAutoSizing = true;
        text.color = new Color(0.07f, 0.16f, 0.18f, 1f);

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

    private static void ConfigurerFond(Image image, Color couleur)
    {
        image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = couleur;
    }

    private static Button CreerBouton(RectTransform parent)
    {
        RectTransform rect = CreerRect(parent, "BoutonContinuer",
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(150f, 42f));
        rect.anchoredPosition = new Vector2(-14f, 14f);
        Image image = rect.gameObject.AddComponent<Image>();
        ConfigurerFond(image, new Color(0.66f, 0.89f, 0.91f, 0.96f));
        RectTransform textRect = CreerRect(rect, "Texte",
            Vector2.zero, Vector2.one, Vector2.zero);
        TMP_Text text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = "Continuer";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 16f;
        text.color = new Color(0.07f, 0.16f, 0.18f, 1f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }
}
