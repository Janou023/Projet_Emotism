using TMPro;
using UnityEngine;

/// <summary>
/// À placer sur le TMP_Dropdown "Taille du texte".
/// Les options doivent être dans cet ordre : Petit, Normal, Grand, Très grand
/// (l'indice de l'option = textSizeLevel).
/// Se rafraîchit aussi quand un réglage change ailleurs (ex. réinitialisation).
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class TextSizeDropdown : MonoBehaviour
{
    TMP_Dropdown dropdown;
    TMP_Text[] textesDropdown;
    float[] taillesInitiales;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(OnChanged);
        textesDropdown = GetComponentsInChildren<TMP_Text>(true);
        taillesInitiales = new float[textesDropdown.Length];
        for (int i = 0; i < textesDropdown.Length; i++)
            taillesInitiales[i] = textesDropdown[i] != null ? textesDropdown[i].fontSize : 0f;
    }

    void OnEnable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged -= Refresh;
    }

    void Refresh()
    {
        if (dropdown == null || SettingsManager.Instance == null) return;
        int level = Mathf.Clamp(SettingsManager.Instance.Data.textSizeLevel, 0, 3);
        dropdown.SetValueWithoutNotify(level);
        float scale = AccessibleText.ScaleFor(level);
        for (int i = 0; i < textesDropdown.Length; i++)
            if (textesDropdown[i] != null)
                textesDropdown[i].fontSize = taillesInitiales[i] * scale;
        AccessibleText.RafraichirTous();
    }

    void OnChanged(int index)
    {
        if (SettingsManager.Instance == null) return;
        SettingsManager.Instance.Data.textSizeLevel = index;
        SettingsManager.Instance.NotifyChanged();
        Refresh();
    }
}
