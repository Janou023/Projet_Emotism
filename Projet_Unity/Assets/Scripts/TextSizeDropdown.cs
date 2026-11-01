using TMPro;
using UnityEngine;

/// <summary>
/// À placer sur le TMP_Dropdown "Taille du texte".
/// Les options doivent être dans cet ordre : Petit, Normal, Grand, Très grand
/// (l'indice de l'option = textSizeLevel).
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class TextSizeDropdown : MonoBehaviour
{
    TMP_Dropdown dropdown;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(OnChanged);
    }

    // À chaque ouverture du menu, on affiche la valeur sauvegardée
    void OnEnable() =>
        dropdown.SetValueWithoutNotify(SettingsManager.Instance.Data.textSizeLevel);

    void OnChanged(int index)
    {
        SettingsManager.Instance.Data.textSizeLevel = index;
        SettingsManager.Instance.NotifyChanged();
    }
}
