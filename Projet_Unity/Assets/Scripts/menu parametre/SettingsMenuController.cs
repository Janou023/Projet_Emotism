using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère les boutons "Retour" et "Réinitialiser" du menu de paramètres.
/// Les méthodes publiques sont appelées depuis l'événement On Click des boutons.
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    [Header("Retour")]
    [Tooltip("Nom exact de la scène à rouvrir. Laisser vide si le menu s'affiche par-dessus le jeu.")]
    [SerializeField] string sceneToReturnTo = "";
    [Tooltip("Objet à masquer au retour quand aucune scène n'est indiquée (ex. le Canvas).")]
    [SerializeField] GameObject menuRoot;

    [Header("Réinitialisation")]
    [SerializeField] GameObject confirmResetPanel;
    [SerializeField] GameObject resetButton;   // reçoit le focus quand on annule
    [SerializeField] GameObject cancelButton;  // reçoit le focus à l'ouverture de la confirmation

    // À chaque ouverture du menu, la fenêtre de confirmation est fermée
    void OnEnable()
    {
        if (confirmResetPanel != null)
            confirmResetPanel.SetActive(false);
    }

    /// <summary>Bouton Retour.</summary>
    public void Back()
    {
        SettingsManager.Instance.Save();

        if (!string.IsNullOrEmpty(sceneToReturnTo))
            SceneManager.LoadScene(sceneToReturnTo);
        else if (menuRoot != null)
            menuRoot.SetActive(false);
    }

    /// <summary>Bouton Réinitialiser : demande d'abord confirmation.</summary>
    public void AskReset()
    {
        confirmResetPanel.SetActive(true);

        // Le focus est mis sur "Non" : appuyer sur Entrée par erreur ne réinitialise rien
        Focus(cancelButton);
    }

    /// <summary>Bouton "Non" de la confirmation.</summary>
    public void CancelReset()
    {
        confirmResetPanel.SetActive(false);
        Focus(resetButton);
    }

    /// <summary>Bouton "Oui" de la confirmation.</summary>
    public void ConfirmReset()
    {
        SettingsManager.Instance.ResetToDefaults(); // sauvegarde et prévient tout le jeu
        confirmResetPanel.SetActive(false);
        Focus(resetButton);
    }

    static void Focus(GameObject target)
    {
        if (target != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(target);
    }
}
