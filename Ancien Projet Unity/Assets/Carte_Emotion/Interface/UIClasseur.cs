using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIClasseur : MonoBehaviour
{
    [Header("Données")]
    public ClasseurEmotions classeurData;

    [Header("Liste des Cartes")]
    public CarteEmotion[] toutesLesCartes = new CarteEmotion[5];

    [Header("Slots UI (8 boutons de cartes)")]
    public Image[] slotImages = new Image[8];

    [Header("Panneau de Zoom")]
    public GameObject inspectionPanel;
    public Image enlargedCardImage;
    public TextMeshProUGUI enlargedCardText; // Optionnel : pour le texteExplicatif

    [Header("Conteneur Principal")]
    public GameObject binderPanel;

    [Header("Positions du carnet")]
    public bool centrerEnModeSelection = true;
    public Vector2 positionSelection;
    public bool definirPositionConsultation;
    public Vector2 positionConsultation;
    [Range(0.5f, 0.9f)] public float positionHorizontaleSelection = 0.76f;
    [Range(0.5f, 0.9f)] public float positionVerticaleSelection = 0.58f;
    [Range(0.1f, 1f)] public float echelleSelectionCompacte = 0.6f;

    [Header("Couleurs")]
    public Color emptySlotColor = new Color(0.85f, 0.85f, 0.85f, 0.4f);

    RectTransform binderRect;
    Vector2 positionInitiale;
    Vector3 echelleInitiale;
    Vector2 ancreMinInitiale;
    Vector2 ancreMaxInitiale;
    Vector2 pivotInitial;
    GameObject fondAssombrissant;
    Canvas canvasCarnet;
    CanvasGroup groupeCanvas;
    CarteEmotion carteGlissee;
    bool depotAccepte;

    void Awake()
    {
        if (binderPanel != null)
        {
            binderRect = binderPanel.GetComponent<RectTransform>();
            if (binderRect != null)
            {
                positionInitiale = binderRect.anchoredPosition;
                echelleInitiale = binderRect.localScale;
                ancreMinInitiale = binderRect.anchorMin;
                ancreMaxInitiale = binderRect.anchorMax;
                pivotInitial = binderRect.pivot;
            }
            Transform fond = ChercherEnfant(binderPanel.transform, "BackgroundDimmer");
            fondAssombrissant = fond != null ? fond.gameObject : null;
        }
        canvasCarnet = GetComponent<Canvas>();
        groupeCanvas = GetComponent<CanvasGroup>();
        if (canvasCarnet != null && groupeCanvas == null)
            groupeCanvas = canvasCarnet.gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        SetupButtonListeners();
        ConfigurerBoutonClasseur();
        RafraichirUI();
    }

    void ConfigurerBoutonClasseur()
    {
        if (FindAnyObjectByType<DialogueUIBootstrap>() != null) return;

        Button bouton = null;
        Button[] boutons = FindObjectsByType<Button>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button candidat in boutons)
        {
            if (candidat.gameObject.name != "BtnClasseur") continue;
            bouton = candidat;
            break;
        }
        if (bouton == null) return;

        bouton.onClick.RemoveListener(OuvrirDepuisBouton);
        bouton.onClick.AddListener(OuvrirDepuisBouton);
    }

    void OuvrirDepuisBouton()
    {
        DefiController defi = FindAnyObjectByType<DefiController>();
        if (defi != null)
        {
            defi.OuvrirClasseur();
            return;
        }

        Ouvrir();
    }

    private void SetupButtonListeners()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] == null) continue;

            Button btn = slotImages[i].GetComponent<Button>();
            if (btn != null)
            {
                int index = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnCardClicked(index));
            }
        }
    }

    // --- Mode sélection (question « quelle émotion ? » dans un défi) ---
    private bool modeSelection;
    private System.Action<CarteEmotion> surCarteChoisie;

    // Ouverture simple (boutons de la scène) : consultation des cartes
    public void Ouvrir()
    {
        Ouvrir(false, null);
    }

    // modeSelection = true : cliquer sur une carte possédée la renvoie à onChoisie, puis ferme le classeur
    public void Ouvrir(bool modeSelection, System.Action<CarteEmotion> onChoisie = null)
    {
        Ouvrir(modeSelection, onChoisie, false);
    }

    public void OuvrirSelectionCompacte(System.Action<CarteEmotion> onChoisie)
    {
        Ouvrir(true, onChoisie, true);
    }

    void Ouvrir(bool modeSelection, System.Action<CarteEmotion> onChoisie, bool compacte)
    {
        if (binderPanel == null)
        {
            Debug.LogError("UIClasseur : Binder Panel n'est pas assigné dans l'Inspector.");
            return;
        }

        this.modeSelection = modeSelection;
        surCarteChoisie = onChoisie;
        if (binderRect == null && binderPanel != null)
            binderRect = binderPanel.GetComponent<RectTransform>();
        if (binderRect != null)
        {
            binderRect.localScale = echelleInitiale;
            if (compacte)
            {
                RectTransform parentRect = binderRect.parent as RectTransform;
                float largeurParent = parentRect != null ? parentRect.rect.width : Screen.width;
                float hauteurParent = parentRect != null ? parentRect.rect.height : Screen.height;
                binderRect.anchorMin = new Vector2(0.5f, 0.5f);
                binderRect.anchorMax = new Vector2(0.5f, 0.5f);
                binderRect.pivot = new Vector2(0.5f, 0.5f);
                binderRect.anchoredPosition = new Vector2(
                    (positionHorizontaleSelection - 0.5f) * largeurParent,
                    (positionVerticaleSelection - 0.5f) * hauteurParent);
                binderRect.localScale = echelleInitiale * echelleSelectionCompacte;
            }
            else
            {
                binderRect.anchorMin = ancreMinInitiale;
                binderRect.anchorMax = ancreMaxInitiale;
                binderRect.pivot = pivotInitial;
                if (modeSelection && centrerEnModeSelection)
                    binderRect.anchoredPosition = positionSelection;
                else if (!modeSelection && definirPositionConsultation)
                    binderRect.anchoredPosition = positionConsultation;
                else if (!modeSelection)
                    binderRect.anchoredPosition = positionInitiale;
            }
        }
        if (fondAssombrissant != null)
            fondAssombrissant.SetActive(modeSelection && !compacte);
        binderPanel.SetActive(true);
        RafraichirUI();
    }

    public void Fermer()
    {
        modeSelection = false;
        surCarteChoisie = null;
        if (binderRect != null)
        {
            binderRect.anchorMin = ancreMinInitiale;
            binderRect.anchorMax = ancreMaxInitiale;
            binderRect.pivot = pivotInitial;
            binderRect.anchoredPosition = positionInitiale;
            binderRect.localScale = echelleInitiale;
        }
        if (groupeCanvas != null) groupeCanvas.blocksRaycasts = true;
        if (fondAssombrissant != null) fondAssombrissant.SetActive(true);
        if (binderPanel != null) binderPanel.SetActive(false);
        if (inspectionPanel != null) inspectionPanel.SetActive(false);
    }

    public bool CommencerGlisserCarte(int index, PointerEventData eventData)
    {
        if (!modeSelection || index < 0 || index >= toutesLesCartes.Length) return false;
        CarteEmotion carte = toutesLesCartes[index];
        if (carte == null || classeurData == null || !classeurData.Possede(carte)) return false;
        if (canvasCarnet == null) canvasCarnet = GetComponent<Canvas>();

        carteGlissee = carte;
        depotAccepte = false;
        if (groupeCanvas != null) groupeCanvas.blocksRaycasts = false;
        return true;
    }

    public bool EstCarteGlissee(CarteEmotionDragHandler handler)
    {
        return handler != null && carteGlissee != null
            && handler.CarteIndex >= 0
            && handler.CarteIndex < toutesLesCartes.Length
            && toutesLesCartes[handler.CarteIndex] == carteGlissee;
    }

    public void GlisserCarte(int index, PointerEventData eventData)
    {
        if (carteGlissee == null || index < 0 || index >= slotImages.Length) return;
        RectTransform carteRect = slotImages[index].rectTransform;
        RectTransform canvasRect = canvasCarnet != null
            ? canvasCarnet.transform as RectTransform
            : null;
        if (canvasRect == null) return;

        Vector2 positionLocale;
        Camera cameraEvenement = canvasCarnet.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : eventData.pressEventCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, cameraEvenement, out positionLocale))
            carteRect.anchoredPosition = positionLocale;
    }

    public void AccepterDepotCarte()
    {
        if (modeSelection && carteGlissee != null) depotAccepte = true;
    }

    public void TerminerGlisserCarte(int index)
    {
        if (groupeCanvas != null) groupeCanvas.blocksRaycasts = true;
        if (index >= 0 && index < slotImages.Length && slotImages[index] != null)
        {
            Transform parent = slotImages[index].transform.parent;
            if (parent != null) LayoutRebuilder.MarkLayoutForRebuild(parent as RectTransform);
        }

        CarteEmotion carte = carteGlissee;
        bool valide = depotAccepte;
        carteGlissee = null;
        depotAccepte = false;

        if (!valide || carte == null) return;
        System.Action<CarteEmotion> callback = surCarteChoisie;
        Fermer();
        if (callback != null) callback(carte);
    }

    Transform ChercherEnfant(Transform parent, string nom)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform enfant = parent.GetChild(i);
            if (enfant.name == nom) return enfant;
            Transform trouve = ChercherEnfant(enfant, nom);
            if (trouve != null) return trouve;
        }
        return null;
    }

    public void RafraichirUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] == null) continue;

            Button btn = slotImages[i].GetComponent<Button>();
            CardHoverEffect hoverEffect = slotImages[i].GetComponent<CardHoverEffect>();

            CarteEmotionDragHandler dragHandler =
                slotImages[i].GetComponent<CarteEmotionDragHandler>();
            if (dragHandler == null)
                dragHandler = slotImages[i].gameObject.AddComponent<CarteEmotionDragHandler>();
            dragHandler.Configurer(this, i);

            if (i < toutesLesCartes.Length && toutesLesCartes[i] != null)
            {
                CarteEmotion carte = toutesLesCartes[i];
                bool possede = (classeurData != null) && classeurData.Possede(carte);

                if (possede)
                {
                    // CARTE DÉBLOQUÉE
                    slotImages[i].sprite = carte.illustration;
                    slotImages[i].color = Color.white;

                    if (btn != null) btn.enabled = true;
                    if (hoverEffect != null) hoverEffect.isUnlocked = true;
                }
                else
                {
                    // CARTE VERROUILLÉE
                    slotImages[i].sprite = carte.lockedSprite;
                    slotImages[i].color = Color.white;

                    if (btn != null) btn.enabled = false;
                    if (hoverEffect != null) hoverEffect.isUnlocked = false;
                }
            }
            else
            {
                // SLOT VIDE
                slotImages[i].color = emptySlotColor;
                if (btn != null) btn.enabled = false;
                if (hoverEffect != null) hoverEffect.isUnlocked = false;
            }
        }
    }

    private void OnCardClicked(int index)
    {
        if (index >= 0 && index < toutesLesCartes.Length)
        {
            CarteEmotion carte = toutesLesCartes[index];

            if (classeurData != null && classeurData.Possede(carte))
            {
                if (inspectionPanel != null && enlargedCardImage != null)
                {
                    enlargedCardImage.sprite = carte.illustration;
                    if (enlargedCardText != null)
                        enlargedCardText.text = carte.texteExplicatif;

                    inspectionPanel.SetActive(true);
                }
            }
        }
    }
    // Ferme uniquement la vue agrandie (zoom) sans fermer le classeur
public void FermerInspection()
{
    if (inspectionPanel != null)
    {
        inspectionPanel.SetActive(false);
    }
}
}