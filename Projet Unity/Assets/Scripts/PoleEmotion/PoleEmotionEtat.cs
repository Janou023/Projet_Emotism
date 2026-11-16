using UnityEngine;

/// <summary>
/// État d'un pôle émotion dans le monde : « à faire » ou « terminé ».
/// Un pôle est terminé quand la carte de son émotion est débloquée (sauvegarde).
/// À placer sur un objet du pôle (ex. un objet vide « EtatPole »).
/// </summary>
public class PoleEmotionEtat : MonoBehaviour
{
    [Tooltip("Identifiant de l'émotion du pôle (ex. colere). Le même que celui de la carte.")]
    [SerializeField] string idEmotion = "colere";

    [Header("Affichés tant que le pôle n'est PAS terminé")]
    [SerializeField] GameObject[] objetsAFaire;

    [Header("Affichés quand le pôle est TERMINÉ")]
    [SerializeField] GameObject[] objetsTermine;

    [Header("Désactivés quand le pôle est terminé")]
    [Tooltip("Ex. le composant DialogueTrigger du PNJ, pour ne pas relancer le pôle.")]
    [SerializeField] Behaviour[] composantsADesactiverSiTermine;

    public bool EstTermine =>
        ProgressManager.Instance != null && ProgressManager.Instance.IsEmotionUnlocked(idEmotion);

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

    void Appliquer()
    {
        bool termine = EstTermine;

        Activer(objetsAFaire, !termine);
        Activer(objetsTermine, termine);

        if (composantsADesactiverSiTermine != null)
        {
            foreach (var c in composantsADesactiverSiTermine)
                if (c != null) c.enabled = !termine;
        }
    }

    static void Activer(GameObject[] objets, bool actif)
    {
        if (objets == null) return;
        foreach (var o in objets)
            if (o != null) o.SetActive(actif);
    }
}
