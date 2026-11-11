using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Gère les onglets du menu : un clic sur un bouton d'onglet affiche la page
/// correspondante et masque les autres.
public class SettingsTabs : MonoBehaviour
{
    [SerializeField] Button[] tabButtons;
    [SerializeField] GameObject[] pages;
    [SerializeField] GameObject[] selectedIndicators; // un trait par onglet (facultatif)

    void Awake()
    {
        if (tabButtons.Length != pages.Length)
        {
            Debug.LogError("[SettingsTabs] Il faut autant de boutons que de pages.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => Show(index));
        }
    }

    void OnEnable() => Show(0);

    public void Show(int index)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            bool selected = i == index;

            pages[i].SetActive(selected);

            if (selectedIndicators != null && i < selectedIndicators.Length && selectedIndicators[i] != null)
                selectedIndicators[i].SetActive(selected);

            var label = tabButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
        }
    }
}
