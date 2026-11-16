using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classeur de cartes émotion.
/// La liste des cartes possédées est désormais enregistrée dans la sauvegarde (ProgressManager)
/// </summary>
public class ClasseurEmotions : MonoBehaviour
{
    // CARTES DE DÉPART : possédées dès le début de la partie (ex. la carte Joie).
    // Les cartes obtenues ensuite sont ajoutées à la sauvegarde, pas à cette liste dans l'éditeur.
    // Laisser vide si le joueur ne doit avoir aucune carte au départ.
    [SerializeField] private List<CarteEmotion> cartesPossedees = new List<CarteEmotion>();

    void Awake()
    {
        // Les cartes de départ sont enregistrées dans la sauvegarde, sans animation
        if (ProgressManager.Instance == null) return;

        foreach (var carte in cartesPossedees)
        {
            if (carte != null) ProgressManager.Instance.UnlockEmotion(carte.id);
        }
    }

    /// <summary>
    /// Ajoute une carte au classeur et l'enregistre dans la sauvegarde.
    /// Renvoie true si la carte est NOUVELLE (le jeu peut alors afficher l'animation d'obtention).
    /// </summary>
    public bool AjouterCarte(CarteEmotion carte)
    {
        if (carte == null) return false;

        bool nouvelle;
        if (ProgressManager.Instance != null)
            nouvelle = ProgressManager.Instance.UnlockEmotion(carte.id);
        else
            nouvelle = !ListeContient(carte.id); // repli : scène testée sans ProgressManager

        if (!ListeContient(carte.id))
            cartesPossedees.Add(carte);

        return nouvelle;
    }

    public bool Possede(CarteEmotion carte)
    {
        if (carte == null) return false;

        if (ProgressManager.Instance != null)
            return ProgressManager.Instance.IsEmotionUnlocked(carte.id);

        return ListeContient(carte.id); // repli : scène testée sans ProgressManager
    }

    /// <summary>Nombre de cartes possédées (d'après la sauvegarde).</summary>
    public int NombreCartes =>
        ProgressManager.Instance != null
            ? ProgressManager.Instance.Data.emotions_terminees.Count
            : cartesPossedees.Count;

    /// <summary>
    /// Reconstruit la liste des cartes possédées à partir de la sauvegarde.
    /// À appeler avec la liste complète des cartes du jeu (UIClasseur.toutesLesCartes).
    /// </summary>
    public void Restaurer(IEnumerable<CarteEmotion> catalogue)
    {
        if (catalogue == null || ProgressManager.Instance == null) return;

        foreach (var carte in catalogue)
        {
            if (carte == null) continue;
            if (ProgressManager.Instance.IsEmotionUnlocked(carte.id) && !ListeContient(carte.id))
                cartesPossedees.Add(carte);
        }
    }

    public List<CarteEmotion> GetCartes()
    {
        return cartesPossedees;
    }

    // Comparaison sans tenir compte des majuscules : "Joie" et "joie" sont la même carte
    bool ListeContient(string id)
    {
        return cartesPossedees.Exists(c => c != null &&
            string.Equals(c.id, id, StringComparison.OrdinalIgnoreCase));
    }
}
