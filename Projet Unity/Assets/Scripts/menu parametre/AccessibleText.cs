using TMPro;
using UnityEngine;

/// <summary>
/// À placer sur chaque texte TextMeshPro qui doit respecter les réglages du joueur
/// (dialogues, cartes émotion, aperçu du menu...).
/// Applique : taille du texte et espacement des lignes.
/// Le texte garde ses valeurs d'origine comme base de calcul.
/// Attention : ne pas activer "Auto Size" sur ces textes.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class AccessibleText : MonoBehaviour
{
    // Multiplicateur de taille pour chaque niveau : Petit, Normal, Grand, Très grand
    static readonly float[] Scales = { 0.85f, 1f, 1.25f, 1.5f };

    // Espacement de lignes ajouté quand le slider est au maximum
    const float MaxExtraLineSpacing = 8f;

    TMP_Text label;
    float baseFontSize;
    float baseLineSpacing;
    bool baseValuesCaptured;

    /// <summary>Utile à l'équipe : multiplicateur de taille pour un niveau donné.</summary>
    public static float ScaleFor(int level) =>
        Scales[Mathf.Clamp(level, 0, Scales.Length - 1)];

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        CapturerValeursInitiales();
    }

    void OnEnable()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogWarning("[AccessibleText] Aucun SettingsManager dans la scène.", this);
            return;
        }

        SettingsManager.Instance.OnSettingsChanged += Apply;
        Apply();
    }

    void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged -= Apply;
    }

    void Apply()
    {
        RafraichirDepuisReglages();
    }

    public void RafraichirDepuisReglages()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (label == null || SettingsManager.Instance == null) return;
        CapturerValeursInitiales();
        var d = SettingsManager.Instance.Data;

        label.fontSize = baseFontSize * ScaleFor(d.textSizeLevel);
        label.lineSpacing = baseLineSpacing + d.lineSpacing * MaxExtraLineSpacing;
    }

    public static void RafraichirTous()
    {
        AccessibleText[] textes = FindObjectsByType<AccessibleText>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (AccessibleText texte in textes)
            if (texte != null) texte.RafraichirDepuisReglages();
    }

    void CapturerValeursInitiales()
    {
        if (baseValuesCaptured || label == null) return;
        baseFontSize = label.fontSize;
        baseLineSpacing = label.lineSpacing;
        baseValuesCaptured = true;
    }
}
