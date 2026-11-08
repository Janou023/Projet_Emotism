using UnityEngine;
using UnityEngine.Audio;

/// Écoute les changements de réglages et applique les volumes à l'AudioMixer.
/// Les noms doivent correspondre exactement aux paramètres exposés dans le mixer.

public class AudioSettingsApplier : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] string masterParam = "MasterVol";
    [SerializeField] string musicParam = "MusicVol";
    [SerializeField] string sfxParam = "SfxVol";

    void OnEnable() => SettingsManager.Instance.OnSettingsChanged += Apply;

    void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged -= Apply;
    }

    void Start() => Apply();

    void Apply()
    {
        var d = SettingsManager.Instance.Data;
        SetVolume(masterParam, d.masterVolume);
        SetVolume(musicParam, d.muteMusic ? 0f : d.musicVolume);
        SetVolume(sfxParam, d.sfxVolume);
    }

    void SetVolume(string param, float linear01)
    {
        // Le mixer travaille en décibels : conversion logarithmique.
        // Mathf.Max évite log10(0) qui donnerait -infini.
        float dB = Mathf.Log10(Mathf.Max(linear01, 0.0001f)) * 20f;
        mixer.SetFloat(param, dB);
    }
}
