using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SliderSetting
{
    MasterVolume,
    MusicVolume,
    SfxVolume,
    TextSpeed,
    LineSpacing
}

/// Se place sur un Slider. Choisis le réglage dans l'Inspector.
/// Le slider lit sa valeur dans SettingsData à l'ouverture, et l'écrit quand on le bouge.
/// L'effet est appliqué par d'autres scripts qui écoutent OnSettingsChanged.
[RequireComponent(typeof(Slider))]
public class SettingSlider : MonoBehaviour
{
    [SerializeField] SliderSetting setting;
    [SerializeField] TMP_Text valueLabel;

    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnEnable() => Refresh();

    void Refresh()
    {
        float value = Read();
        slider.SetValueWithoutNotify(value);
        UpdateLabel(value);
    }

    void OnSliderChanged(float value)
    {
        Write(value);
        UpdateLabel(value);
        SettingsManager.Instance.NotifyChanged();
    }

    void UpdateLabel(float value)
    {
        if (valueLabel != null)
            valueLabel.text = Mathf.RoundToInt(value * 100f) + " %";
    }

    float Read()
    {
        var d = SettingsManager.Instance.Data;
        switch (setting)
        {
            case SliderSetting.MasterVolume: return d.masterVolume;
            case SliderSetting.MusicVolume:  return d.musicVolume;
            case SliderSetting.SfxVolume:    return d.sfxVolume;
            case SliderSetting.TextSpeed:    return d.textSpeed;
            case SliderSetting.LineSpacing:  return d.lineSpacing;
            default: return 0f;
        }
    }

    void Write(float v)
    {
        var d = SettingsManager.Instance.Data;
        switch (setting)
        {
            case SliderSetting.MasterVolume: d.masterVolume = v; break;
            case SliderSetting.MusicVolume:  d.musicVolume = v; break;
            case SliderSetting.SfxVolume:    d.sfxVolume = v; break;
            case SliderSetting.TextSpeed:    d.textSpeed = v; break;
            case SliderSetting.LineSpacing:  d.lineSpacing = v; break;
        }
    }
}
