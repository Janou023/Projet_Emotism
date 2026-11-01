using UnityEngine;

/// <summary>
/// Écoute les réglages et applique le mode plein écran / fenêtré.
/// À placer sur le même objet que le SettingsManager.
/// Attention : dans l'éditeur Unity, le plein écran n'a aucun effet visible.
/// Il ne se teste que dans un build du jeu.
/// </summary>
public class ScreenSettingsApplier : MonoBehaviour
{
    void OnEnable() => SettingsManager.Instance.OnSettingsChanged += Apply;

    void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged -= Apply;
    }

    void Start() => Apply();

    void Apply()
    {
        bool wantFullscreen = SettingsManager.Instance.Data.fullscreen;
        bool isFullscreen = Screen.fullScreenMode != FullScreenMode.Windowed;

        // Cette méthode est appelée à chaque réglage modifié (volumes...) :
        // on ne touche à l'écran que si le mode doit vraiment changer
        if (wantFullscreen == isFullscreen) return;

        Screen.fullScreenMode = wantFullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

#if UNITY_EDITOR
        Debug.Log($"[Ecran] Plein écran demandé : {wantFullscreen} (effet visible uniquement dans un build)");
#endif
    }
}
