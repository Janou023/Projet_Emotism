using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Point d'accès unique à la progression : ProgressManager.Instance
/// - charge et sauvegarde sauvegarde.json
/// - débloque les cartes émotion et enregistre les défis terminés
/// - prévient le jeu (classeur, défis...) par des événements
/// UN SEUL script doit écrire sauvegarde.json : celui-ci.
/// </summary>
[DefaultExecutionOrder(-90)] // juste après SettingsManager
public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    public ProgressData Data { get; private set; } = new ProgressData();

    /// <summary>Déclenché quand une carte vient d'être débloquée (avec son identifiant).</summary>
    public event Action<string> OnEmotionUnlocked;

    /// <summary>Déclenché quand un défi vient d'être terminé (avec son identifiant).</summary>
    public event Action<string> OnChallengeCompleted;

    /// <summary>Déclenché à chaque modification de la progression.</summary>
    public event Action OnProgressChanged;

    string SavePath => Path.Combine(Application.persistentDataPath, "sauvegarde.json");

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    // ---------- CARTES ÉMOTION ----------

    public bool IsEmotionUnlocked(string id) => Data.emotions_terminees.Contains(id);

    /// <summary>True si TOUTES les cartes demandées sont débloquées (utile pour les défis).</summary>
    public bool HasAllEmotions(params string[] ids)
    {
        foreach (var id in ids)
            if (!IsEmotionUnlocked(id)) return false;
        return true;
    }

    /// <summary>
    /// Débloque une carte. Renvoie true si elle est NOUVELLE, false si elle l'était déjà
    /// (permet de ne montrer l'animation d'obtention qu'une seule fois).
    /// </summary>
    public bool UnlockEmotion(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning("[Progression] Identifiant de carte vide : la carte n'a pas été débloquée.");
            return false;
        }
        if (IsEmotionUnlocked(id)) return false;

        Data.emotions_terminees.Add(id);
        Save(); // on sauvegarde tout de suite : la progression ne doit jamais se perdre

        OnEmotionUnlocked?.Invoke(id);
        OnProgressChanged?.Invoke();
        return true;
    }

    // ---------- DÉFIS ----------

    public bool IsChallengeCompleted(string id) => Data.defis_termines.Contains(id);

    /// <summary>Marque un défi comme terminé. Renvoie true s'il est nouveau.</summary>
    public bool CompleteChallenge(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || IsChallengeCompleted(id)) return false;

        Data.defis_termines.Add(id);
        Save();

        OnChallengeCompleted?.Invoke(id);
        OnProgressChanged?.Invoke();
        return true;
    }

    // ---------- POSITION ----------

    public void SavePosition(string carte, float x, float y)
    {
        Data.position.carte = carte;
        Data.position.x = x;
        Data.position.y = y;
        Save();
    }

    // ---------- NOUVELLE PARTIE ----------

    /// <summary>Efface la progression (pas les réglages d'accessibilité).</summary>
    [ContextMenu("Réinitialiser la progression")]
    public void ResetProgress()
    {
        Data = new ProgressData();
        Save();
        OnProgressChanged?.Invoke();
    }

    // ---------- FICHIER ----------

    public void Save()
    {
        try
        {
            // On écrit dans un fichier temporaire puis on le déplace : si le jeu plante
            // pendant l'écriture, l'ancienne sauvegarde reste intacte
            string tmp = SavePath + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(Data, true));
            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(tmp, SavePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Progression] Sauvegarde impossible : {e.Message}");
        }
    }

    void Load()
    {
        Data = new ProgressData();

        try
        {
            if (File.Exists(SavePath))
            {
                var loaded = JsonUtility.FromJson<ProgressData>(File.ReadAllText(SavePath));
                if (loaded != null) Data = loaded;
            }
        }
        catch (Exception e)
        {
            // Fichier invalide (ex. commentaire "//" dans le JSON) : on repart de zéro
            Debug.LogWarning($"[Progression] Lecture impossible, progression vide utilisée : {e.Message}");
            Data = new ProgressData();
        }

        Data.Sanitize();
        OnProgressChanged?.Invoke();
    }

    void OnApplicationPause(bool paused) { if (paused) Save(); }
    void OnApplicationQuit() => Save();
}
