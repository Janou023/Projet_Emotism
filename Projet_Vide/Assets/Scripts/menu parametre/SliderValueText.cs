using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// À placer sur un Slider. Affiche sa valeur (0 à 1) en pourcentage dans un texte TMP.

[RequireComponent(typeof(Slider))]
public class SliderValueText : MonoBehaviour
{
    [SerializeField] TMP_Text valueText;

    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(UpdateText);
    }

    void OnEnable()
    {
        UpdateText(slider.value);
    }

    void UpdateText(float value)
    {
        if (valueText != null)
            valueText.text = Mathf.RoundToInt(value * 100f) + " %";
    }
}
