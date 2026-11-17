using System;
using System.IO;
using UnityEngine;

/// Point d'accès unique aux réglages : SettingsManager.Instance.Data
/// - charge et sauvegarde le fichier reglages.json
/// - prévient les autres scripts quand un réglage change (événement)
[DefaultExecutionOrder(-100)] // s'exécute avant les autres scripts
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public SettingsData Data { get; private set; } = new SettingsData();

    ///Déclenché à chaque changement. Les scripts du jeu s'y abonnent.
    public event Action OnSettingsChanged;

    string SavePath => Path.Combine(Application.persistentDataPath, "reglages.json");

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

    ///À appeler après avoir modifié Data : prévient le jeu puis sauvegarde.
    public void NotifyChanged()
    {
        OnSettingsChanged?.Invoke();
        Save();
    }

    public void ResetToDefaults()
    {
        Data.ResetToDefaults();
        NotifyChanged();
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Reglages] Sauvegarde impossible : {e.Message}");
        }
    }

    void Load()
    {
        Data = new SettingsData();

        try
        {
            if (File.Exists(SavePath))
            {
                // Les champs absents du fichier gardent leur valeur par défaut
                JsonUtility.FromJsonOverwrite(File.ReadAllText(SavePath), Data);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Reglages] Lecture impossible, valeurs par défaut utilisées : {e.Message}");
            Data = new SettingsData();
        }

        Data.Sanitize();
        OnSettingsChanged?.Invoke();
    }

    void OnApplicationPause(bool paused) { if (paused) Save(); }
    void OnApplicationQuit() => Save();
}
