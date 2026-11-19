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
        InitialiserCartesDeDepart();
    }

    public void InitialiserCartesDeDepart()
    {
        // Appelée aussi à l'ouverture du carnet au cas où le ProgressManager
        // n'était pas encore initialisé pendant Awake.
        if (ProgressManager.Instance == null) return;

        foreach (var carte in cartesPossedees)
        {
            if (carte != null && !string.IsNullOrWhiteSpace(carte.id))
                ProgressManager.Instance.UnlockEmotion(carte.id);
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
    /// Remplace les entrées possédées par les fiches correspondantes du catalogue,
    /// pour que les cartes possédées aient aussi leurs sprites et leur description.
    /// </summary>
    public void Restaurer(IEnumerable<CarteEmotion> catalogue)
    {
        if (catalogue == null) return;

        List<CarteEmotion> cartesCatalogue = new List<CarteEmotion>(catalogue);
        List<CarteEmotion> cartesRestaurees = new List<CarteEmotion>();
        foreach (var carte in cartesCatalogue)
        {
            if (carte == null || string.IsNullOrWhiteSpace(carte.id)) continue;

            bool estPossedee = ProgressManager.Instance != null
                ? ProgressManager.Instance.IsEmotionUnlocked(carte.id)
                : ListeContient(carte.id);

            bool dejaAjoutee = cartesRestaurees.Exists(c =>
                ProgressData.Normalize(c.id) == ProgressData.Normalize(carte.id));
            if (estPossedee && !dejaAjoutee)
                cartesRestaurees.Add(carte);
        }

        cartesPossedees.Clear();
        cartesPossedees.AddRange(cartesRestaurees);
    }

    public List<CarteEmotion> GetCartes()
    {
        return cartesPossedees;
    }

    // Compare les identifiants sans tenir compte des majuscules ni des accents.
    bool ListeContient(string id)
    {
        string idNormalise = ProgressData.Normalize(id);
        if (idNormalise == "") return false;

        return cartesPossedees.Exists(c => c != null &&
            ProgressData.Normalize(c.id) == idNormalise);
    }
}
