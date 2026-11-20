using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Bouton « Nouvelle partie » : efface la progression (cartes, défis, position) après confirmation,
/// puis recharge la scène de départ. Les réglages d'accessibilité sont conservés.
/// Les méthodes publiques sont appelées depuis l'événement On Click des boutons.
/// </summary>
public class NouvellePartie : MonoBehaviour
{
    [Tooltip("Nom exact de la scène de départ du jeu (elle doit être dans Build Profiles).")]
    [SerializeField] string sceneDepart = "";

    [Header("Confirmation")]
    [SerializeField] GameObject confirmationPanel;
    [Tooltip("Le bouton « Non » : il reçoit le focus à l'ouverture, pour qu'un appui sur Entrée ne suffise pas à tout effacer.")]
    [SerializeField] GameObject boutonNon;
    [SerializeField] GameObject boutonNouvellePartie;   // reçoit le focus quand on annule

    void OnEnable()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    /// <summary>Bouton « Nouvelle partie » : demande d'abord confirmation.</summary>
    public void Demander()
    {
        if (confirmationPanel == null)
        {
            Debug.LogError("[NouvellePartie] Le panneau de confirmation n'est pas assigné.");
            return;
        }

        confirmationPanel.SetActive(true);
        Focus(boutonNon);
    }

    /// <summary>Bouton « Non ».</summary>
    public void Annuler()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
        Focus(boutonNouvellePartie);
    }

    /// <summary>Bouton « Oui » : efface la progression et recommence.</summary>
    public void Confirmer()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.ResetProgress();
        else
            Debug.LogWarning("[NouvellePartie] Aucun ProgressManager dans la scène : rien n'a été effacé.");

        GestionnaireTutoriels.Instance.ResetAll();

        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        if (string.IsNullOrEmpty(sceneDepart))
        {
            Debug.LogWarning("[NouvellePartie] Aucune scène de départ indiquée : la progression est effacée, " +
                             "mais la scène actuelle n'est pas rechargée.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneDepart))
        {
            Debug.LogError("[NouvellePartie] Scène absente des scènes de build : " + sceneDepart);
            return;
        }

        // On recharge la scène : les cartes de départ (ex. Joie) sont alors réenregistrées
        if (FadeManager.Instance != null)
            FadeManager.Instance.LoadSceneWithFade(sceneDepart);
        else
            SceneManager.LoadScene(sceneDepart);
    }

    static void Focus(GameObject cible)
    {
        if (cible != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(cible);
    }
}
