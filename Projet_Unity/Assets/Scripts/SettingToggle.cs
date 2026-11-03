using UnityEngine;
using UnityEngine.UI;

public enum ToggleSetting
{
    MuteMusic,
    Fullscreen,
    InstantText,
    ConfirmCardChoice
}

/// <summary>
/// Se place sur un Toggle (case à cocher). Choisis le réglage dans l'Inspector.
/// Lit la valeur dans SettingsData, l'écrit quand on coche ou décoche,
/// et se rafraîchit quand un réglage change ailleurs (ex. réinitialisation).
/// L'effet est appliqué par d'autres scripts qui écoutent OnSettingsChanged.
/// </summary>
[RequireComponent(typeof(Toggle))]
public class SettingToggle : MonoBehaviour
{
    [SerializeField] ToggleSetting setting;

    Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnEnable()
    {
        SettingsManager.Instance.OnSettingsChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged -= Refresh;
    }

    void Refresh() => toggle.SetIsOnWithoutNotify(Read());

    void OnToggleChanged(bool value)
    {
        Write(value);
        SettingsManager.Instance.NotifyChanged();
    }

    bool Read()
    {
        var d = SettingsManager.Instance.Data;
        switch (setting)
        {
            case ToggleSetting.MuteMusic: return d.muteMusic;
            case ToggleSetting.Fullscreen: return d.fullscreen;
            case ToggleSetting.InstantText: return d.instantText;
            case ToggleSetting.ConfirmCardChoice: return d.confirmCardChoice;
            default: return false;
        }
    }

    void Write(bool v)
    {
        var d = SettingsManager.Instance.Data;
        switch (setting)
        {
            case ToggleSetting.MuteMusic: d.muteMusic = v; break;
            case ToggleSetting.Fullscreen: d.fullscreen = v; break;
            case ToggleSetting.InstantText: d.instantText = v; break;
            case ToggleSetting.ConfirmCardChoice: d.confirmCardChoice = v; break;
        }
    }
}
