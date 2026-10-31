using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Couleurs")]
    public Color emptySlotColor = new Color(0.85f, 0.85f, 0.85f, 0.4f);

    void Start()
    {
        SetupButtonListeners();
        RafraichirUI();
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

    public void Ouvrir()
    {
        if (binderPanel != null) binderPanel.SetActive(true);
        RafraichirUI();
    }

    public void Fermer()
    {
        if (binderPanel != null) binderPanel.SetActive(false);
        if (inspectionPanel != null) inspectionPanel.SetActive(false);
    }

    public void RafraichirUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] == null) continue;

            Button btn = slotImages[i].GetComponent<Button>();
            CardHoverEffect hoverEffect = slotImages[i].GetComponent<CardHoverEffect>();

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