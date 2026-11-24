using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>Construit et fait défiler l'écran de remerciements de fin de jeu.</summary>
public class CreditsScreenController : MonoBehaviour
{
    [SerializeField] float vitesseDefilement = 30f;
    [SerializeField] string sceneMenuPrincipal = "MenuPrincipal";
    [SerializeField] string sceneMonde = "Monde";
    [SerializeField] VideoClip fondMenuPrincipal;
    [SerializeField] string[] participants =
    {
        "Alex FIORETZI--YSABEL",
        "Ambre SOUCHON-BECHAREL",
        "Caroline PROMIS",
        "Fleur AFFOLTER-CAUVIN",
        "Jan DE GRAEVE",
        "Léna SERIN",
        "Paolo KEHL",
        "Zoé SCHLIEFER BERLIOUX"
    };

    ScrollRect defilement;
    RectTransform contenu;
    RectTransform viewport;
    Button boutonPasser;
    GameObject actionsFinales;
    Coroutine defilementEnCours;

    void Start()
    {
        ConstruireEcran();
        defilementEnCours = StartCoroutine(DefilerCredits());
    }

    void ConstruireEcran()
    {
        CreerFondAnime();

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        AssurerEventSystem();

        Image fond = CreerImage("Fond", transform);
        Etirer(fond.rectTransform, Vector2.zero, Vector2.zero);
        fond.color = new Color(0f, 0f, 0f, 0.5882353f);

        TMP_Text titre = CreerTexte("Titre", transform, "Merci d'avoir joué !", 58f,
            new Color(0.96f, 0.86f, 0.69f, 1f), FontStyles.Bold);
        PlacerEnHaut(titre.rectTransform, 36f, 100f);

        TMP_Text sousTitre = CreerTexte("SousTitre", transform,
            "L'équipe d'Emotism vous remercie pour votre partie.", 26f,
            new Color(0.91f, 0.87f, 0.91f, 1f), FontStyles.Normal);
        PlacerEnHaut(sousTitre.rectTransform, 116f, 54f);

        GameObject zone = new GameObject("ZoneCredits", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
        zone.transform.SetParent(transform, false);
        RectTransform zoneRect = zone.GetComponent<RectTransform>();
        zoneRect.anchorMin = Vector2.zero;
        zoneRect.anchorMax = Vector2.one;
        zoneRect.offsetMin = new Vector2(90f, 130f);
        zoneRect.offsetMax = new Vector2(-90f, -190f);
        zone.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.025f);

        viewport = zoneRect;
        defilement = zone.GetComponent<ScrollRect>();
        defilement.viewport = viewport;
        defilement.horizontal = false;
        defilement.vertical = true;
        defilement.inertia = false;
        defilement.movementType = ScrollRect.MovementType.Clamped;

        GameObject contenuObjet = new GameObject("ContenuCredits", typeof(RectTransform));
        contenuObjet.transform.SetParent(zone.transform, false);
        contenu = contenuObjet.GetComponent<RectTransform>();
        contenu.anchorMin = new Vector2(0f, 1f);
        contenu.anchorMax = Vector2.one;
        contenu.pivot = new Vector2(0.5f, 1f);
        contenu.sizeDelta = new Vector2(0f, 1600f);
        defilement.content = contenu;

        TMP_Text texte = CreerTexte("NomsCredits", contenu,
            ConstruireTexteCredits(), 34f, new Color(0.95f, 0.91f, 0.94f, 1f), FontStyles.Normal);
        texte.alignment = TextAlignmentOptions.Center;
        texte.lineSpacing = 9f;
        texte.margin = new Vector4(40f, 24f, 40f, 24f);
        RectTransform texteRect = texte.rectTransform;
        texteRect.anchorMin = Vector2.zero;
        texteRect.anchorMax = Vector2.one;
        texteRect.offsetMin = Vector2.zero;
        texteRect.offsetMax = Vector2.zero;
        if (SettingsManager.Instance != null)
            texte.fontSize *= AccessibleText.ScaleFor(SettingsManager.Instance.Data.textSizeLevel);
        texte.ForceMeshUpdate();
        contenu.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            Mathf.Max(viewport.rect.height + 1f, texte.preferredHeight));

        boutonPasser = CreerBouton("Passer les crédits", transform, new Vector2(1f, 0f),
            new Vector2(-145f, 62f), PasserCredits);
        actionsFinales = new GameObject("ActionsFinales", typeof(RectTransform));
        actionsFinales.transform.SetParent(transform, false);
        RectTransform actionsRect = actionsFinales.GetComponent<RectTransform>();
        actionsRect.anchorMin = new Vector2(0.5f, 0f);
        actionsRect.anchorMax = new Vector2(0.5f, 0f);
        actionsRect.pivot = new Vector2(0.5f, 0f);
        actionsRect.anchoredPosition = new Vector2(0f, 32f);
        actionsRect.sizeDelta = new Vector2(700f, 74f);
        actionsFinales.SetActive(false);
        CreerBouton("Menu principal", actionsFinales.transform, new Vector2(0f, 0.5f),
            new Vector2(165f, 0f), RetourMenuPrincipal);
        CreerBouton("Retour au monde", actionsFinales.transform, new Vector2(1f, 0.5f),
            new Vector2(-165f, 0f), RetourMonde);

        defilement.verticalNormalizedPosition = 1f;
    }

    void CreerFondAnime()
    {
        if (fondMenuPrincipal == null) return;

        GameObject objetCamera = new GameObject("CameraFondCredits");
        Camera cameraFond = objetCamera.AddComponent<Camera>();
        cameraFond.clearFlags = CameraClearFlags.SolidColor;
        cameraFond.backgroundColor = Color.black;
        cameraFond.orthographic = true;
        cameraFond.orthographicSize = 5f;
        cameraFond.depth = -1f;
        objetCamera.transform.position = new Vector3(0f, 0f, -10f);

        VideoPlayer video = objetCamera.AddComponent<VideoPlayer>();
        video.clip = fondMenuPrincipal;
        video.renderMode = VideoRenderMode.CameraNearPlane;
        video.targetCamera = cameraFond;
        video.aspectRatio = VideoAspectRatio.FitOutside;
        video.isLooping = true;
        video.playOnAwake = true;
    }

    string ConstruireTexteCredits()
    {
        string liste = participants != null ? string.Join("\n\n", participants) : "";
        return "\n\n\n\n\n\n" +
            "CRÉATION DU JEU\n\n" + liste + "\n\n\n\n\n\n" +
            "Merci d'avoir joué à Emotism !!.\n\n" +
            "Prenez soin de vous :) A bientôt !\n\n\n\n\n\n";
    }

    IEnumerator DefilerCredits()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        while (defilement != null && defilement.verticalNormalizedPosition > 0f)
        {
            float distance = Mathf.Max(1f, contenu.rect.height - viewport.rect.height);
            float delta = vitesseDefilement * Time.unscaledDeltaTime / distance;
            defilement.verticalNormalizedPosition = Mathf.Max(0f,
                defilement.verticalNormalizedPosition - delta);
            yield return null;
        }
        AfficherActionsFinales();
    }

    void PasserCredits()
    {
        if (defilementEnCours != null) StopCoroutine(defilementEnCours);
        AfficherActionsFinales();
    }

    void AfficherActionsFinales()
    {
        if (boutonPasser != null) boutonPasser.gameObject.SetActive(false);
        if (actionsFinales != null) actionsFinales.SetActive(true);
    }

    void RetourMenuPrincipal()
    {
        if (ProgressManager.Instance != null) ProgressManager.Instance.Save();
        if (SettingsManager.Instance != null) SettingsManager.Instance.Save();
        SceneManager.LoadScene(sceneMenuPrincipal);
    }

    void RetourMonde()
    {
        SceneManager.LoadScene(sceneMonde);
    }

    static TMP_Text CreerTexte(string nom, Transform parent, string contenuTexte,
        float taille, Color couleur, FontStyles style)
    {
        GameObject objet = new GameObject(nom, typeof(RectTransform), typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        objet.transform.SetParent(parent, false);
        TMP_Text texte = objet.GetComponent<TMP_Text>();
        texte.text = contenuTexte;
        texte.fontSize = taille;
        texte.color = couleur;
        texte.fontStyle = style;
        texte.alignment = TextAlignmentOptions.Center;
        texte.textWrappingMode = TextWrappingModes.Normal;
        texte.raycastTarget = false;
        return texte;
    }

    static Image CreerImage(string nom, Transform parent)
    {
        GameObject objet = new GameObject(nom, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objet.transform.SetParent(parent, false);
        return objet.GetComponent<Image>();
    }

    static Button CreerBouton(string etiquette, Transform parent, Vector2 ancre,
        Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject objet = new GameObject("Bouton_" + etiquette.Replace(' ', '_'),
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        objet.transform.SetParent(parent, false);
        RectTransform rect = objet.GetComponent<RectTransform>();
        rect.anchorMin = ancre;
        rect.anchorMax = ancre;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(280f, 62f);
        Image fond = objet.GetComponent<Image>();
        fond.color = new Color(0.75f, 0.49f, 0.34f, 1f);
        Button bouton = objet.GetComponent<Button>();
        bouton.targetGraphic = fond;
        bouton.onClick.AddListener(action);

        TMP_Text texte = CreerTexte("Texte", objet.transform, etiquette, 23f,
            Color.white, FontStyles.Bold);
        Etirer(texte.rectTransform, Vector2.zero, Vector2.zero);
        return bouton;
    }

    static void PlacerEnHaut(RectTransform rect, float margeHaut, float hauteur)
    {
        rect.anchorMin = new Vector2(0.05f, 1f);
        rect.anchorMax = new Vector2(0.95f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -margeHaut);
        rect.sizeDelta = new Vector2(0f, hauteur);
    }

    static void Etirer(RectTransform rect, Vector2 margeMin, Vector2 margeMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = margeMin;
        rect.offsetMax = -margeMax;
    }

    static void AssurerEventSystem()
    {
        if (EventSystem.current != null) return;
        GameObject objet = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        InputSystemUIInputModule module = objet.AddComponent<InputSystemUIInputModule>();
        module.AssignDefaultActions();
#else
        objet.AddComponent<StandaloneInputModule>();
#endif
    }
}
