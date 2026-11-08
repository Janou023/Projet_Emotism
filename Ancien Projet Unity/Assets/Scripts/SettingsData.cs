using System;
using UnityEngine;

/// Tous les réglages du joueur. Aucune logique de jeu, que des données.
[Serializable]
public class SettingsData
{
    public int version = 1;

    public float masterVolume = 0.8f;
    public float musicVolume = 0.5f;
    public float sfxVolume = 0.7f;
    public bool muteMusic = false;

    public int textSizeLevel = 1;      // 0 petit, 1 normal, 2 grand, 3 très grand
    public float lineSpacing = 0f;
    public bool fullscreen = true;

    public float textSpeed = 0.5f;
    public bool instantText = false;
    public bool confirmCardChoice = true;

    ///Remet toutes les valeurs par défaut.
    public void ResetToDefaults()
    {
        var d = new SettingsData();
        foreach (var field in typeof(SettingsData).GetFields())
            field.SetValue(this, field.GetValue(d));
    }

    /// Remet dans des valeurs valides tout ce qui sort des limites.
    /// Utile si quelqu'un modifie le fichier JSON à la main.
    public void Sanitize()
    {
        masterVolume = Mathf.Clamp01(masterVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        lineSpacing = Mathf.Clamp01(lineSpacing);
        textSpeed = Mathf.Clamp01(textSpeed);
        textSizeLevel = Mathf.Clamp(textSizeLevel, 0, 3);
    }
}
