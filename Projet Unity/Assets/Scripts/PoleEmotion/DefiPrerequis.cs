using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Verrouille un défi tant que le joueur n'a pas toutes les cartes émotion dont il a besoin.
/// Les cartes demandées sont détectées AUTOMATIQUEMENT dans le JSON du défi
/// (toutes les questions « quelle émotion ? »), il n'y a donc rien à recopier à la main.
/// À placer sur le PNJ ou l'objet du défi.
/// </summary>
public class DefiPrerequis : MonoBehaviour
{
    [Tooltip("Le fichier JSON du défi (le même que dans le DialogueTrigger).")]
    [SerializeField] TextAsset jsonDefi;

    [Tooltip("Facultatif : cartes exigées en plus de celles du JSON (ex. colere).")]
    [SerializeField] string[] cartesSupplementaires;

    [Header("Quand le défi est VERROUILLÉ")]
    [Tooltip("Affichés tant que le défi est verrouillé (panneau, cadenas...).")]
    [SerializeField] GameObject[] objetsVerrouille;

    [Tooltip("Désactivés tant que le défi est verrouillé : ajouter le composant DialogueTrigger.")]
    [SerializeField] Behaviour[] composantsADesactiverSiVerrouille;

    readonly List<string> requises = new List<string>();

    public bool EstDeverrouille => CartesManquantes().Count == 0;

    void Awake()
    {
        CalculerCartesRequises();
    }

    void OnEnable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged += Appliquer;
        Appliquer();
    }

    void OnDisable()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnProgressChanged -= Appliquer;
    }

    void CalculerCartesRequises()
    {
        requises.Clear();

        if (jsonDefi != null)
        {
            try
            {
                foreach (var etape in EtapeDefi.Charger(jsonDefi.text).Values)
                {
                    if (etape.type == TypeQuestion.CarteEmotion)
                        Ajouter(etape.emotionAttendue);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[DefiPrerequis] Lecture du JSON impossible : " + e.Message, this);
            }
        }

        if (cartesSupplementaires != null)
        {
            foreach (var id in cartesSupplementaires)
                Ajouter(id);
        }
    }

    void Ajouter(string id)
    {
        string cle = ProgressData.Normalize(id);
        if (cle != "" && !requises.Contains(cle))
            requises.Add(cle);
    }

    /// <summary>Cartes encore à obtenir (liste vide = défi accessible).</summary>
    public List<string> CartesManquantes()
    {
        var manquantes = new List<string>();

        // Scène testée sans ProgressManager : on ne bloque rien
        if (ProgressManager.Instance == null) return manquantes;

        foreach (var id in requises)
        {
            if (!ProgressManager.Instance.IsEmotionUnlocked(id))
                manquantes.Add(id);
        }
        return manquantes;
    }

    void Appliquer()
    {
        bool verrouille = !EstDeverrouille;

        if (objetsVerrouille != null)
        {
            foreach (var o in objetsVerrouille)
                if (o != null) o.SetActive(verrouille);
        }

        if (composantsADesactiverSiVerrouille != null)
        {
            foreach (var c in composantsADesactiverSiVerrouille)
                if (c != null) c.enabled = !verrouille;
        }
    }

    [ContextMenu("Afficher les cartes manquantes")]
    void AfficherCartesManquantes()
    {
        CalculerCartesRequises();
        Debug.Log("[DefiPrerequis] Cartes requises : " + string.Join(", ", requises) +
                  " | manquantes : " + string.Join(", ", CartesManquantes()));
    }
}
