using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gère les boutons "Retour" et "Réinitialiser" du menu de paramètres.
/// Les méthodes publiques sont appelées depuis l'événement On Click des boutons.
/// Le bouton Retour se ferme TOUJOURS, même si un élément est mal relié.
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    [Header("Retour")]
    [Tooltip("Nom exact de la scène à rouvrir. Laisser vide si le menu s'affiche par-dessus le jeu.")]
    [SerializeField] string sceneToReturnTo = "";
    [Tooltip("Objet à masquer au retour. Vide = le Canvas qui contient ce menu.")]
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

    void Start()
    {
        DesactiverCameraLocaleSiMenuConserve();
        AjouterCurseurAmbiance();
    }

    void DesactiverCameraLocaleSiMenuConserve()
    {
        Scene sceneRetour = SceneManager.GetSceneByName(sceneToReturnTo);
        if (!sceneRetour.IsValid() || !sceneRetour.isLoaded || sceneRetour == gameObject.scene)
            return;

        foreach (Camera camera in FindObjectsByType<Camera>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (camera.gameObject.scene == gameObject.scene)
                camera.enabled = false;

        foreach (AudioListener listener in FindObjectsByType<AudioListener>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (listener.gameObject.scene == gameObject.scene)
                listener.enabled = false;
    }

    void AjouterCurseurAmbiance()
    {
        SettingSlider[] curseurs = FindObjectsByType<SettingSlider>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (SettingSlider curseur in curseurs)
            if (curseur.Setting == SliderSetting.AmbienceVolume) return;

        SettingSlider curseurMusique = null;
        foreach (SettingSlider curseur in curseurs)
            if (curseur.Setting == SliderSetting.MusicVolume)
            {
                curseurMusique = curseur;
                break;
            }
        if (curseurMusique == null) return;

        Transform ligneMusique = curseurMusique.transform;
        while (ligneMusique.parent != null && ligneMusique.parent.name != "Content")
            ligneMusique = ligneMusique.parent;
        if (ligneMusique.parent == null) return;

        GameObject ligneAmbiance = Instantiate(ligneMusique.gameObject, ligneMusique.parent, false);
        ligneAmbiance.name = "Ligne_Ambiance";
        ligneAmbiance.transform.SetSiblingIndex(ligneMusique.GetSiblingIndex() + 1);

        foreach (TMP_Text texte in ligneAmbiance.GetComponentsInChildren<TMP_Text>(true))
            if (texte.text == "Musique") texte.text = "Ambiance";

        SettingSlider curseurAmbiance = ligneAmbiance.GetComponentInChildren<SettingSlider>(true);
        if (curseurAmbiance != null)
            curseurAmbiance.Configure(SliderSetting.AmbienceVolume);
    }

    /// <summary>Bouton Retour.</summary>
    public void Back()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.Save();
        else
            Debug.LogWarning("[Parametres] Aucun SettingsManager dans la scène : les réglages ne peuvent pas être sauvegardés.");

        if (!string.IsNullOrEmpty(sceneToReturnTo))
        {
            Scene sceneRetour = SceneManager.GetSceneByName(sceneToReturnTo);
            if (sceneRetour.IsValid() && sceneRetour.isLoaded && sceneRetour != gameObject.scene)
            {
                MenuPrincipal menu = FindAnyObjectByType<MenuPrincipal>();
                if (menu != null)
                    menu.FermerParametres(gameObject.scene);
                else
                {
                    SceneManager.SetActiveScene(sceneRetour);
                    SceneManager.UnloadSceneAsync(gameObject.scene);
                }
            }
            else
            {
            {
            SceneManager.LoadScene(sceneToReturnTo);
            }
        }
            return;
        }

        GameObject racine = RacineMenu();
        if (racine != null)
            racine.SetActive(false);
        else
            Debug.LogWarning("[Parametres] Rien à fermer : Menu Root est vide et aucun Canvas n'a été trouvé.");
    }

    /// <summary>Bouton Réinitialiser : demande d'abord confirmation.</summary>
    public void AskReset()
    {
        if (confirmResetPanel == null)
        {
            Debug.LogWarning("[Parametres] Confirm Reset Panel n'est pas assigné.");
            return;
        }

        confirmResetPanel.SetActive(true);

        // Le focus est mis sur "Non" : appuyer sur Entrée par erreur ne réinitialise rien
        Focus(cancelButton);
    }

    /// <summary>Bouton "Non" de la confirmation.</summary>
    public void CancelReset()
    {
        if (confirmResetPanel != null)
            confirmResetPanel.SetActive(false);
        Focus(resetButton);
    }

    /// <summary>Bouton "Oui" de la confirmation.</summary>
    public void ConfirmReset()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.ResetToDefaults(); // sauvegarde et prévient tout le jeu
        else
            Debug.LogWarning("[Parametres] Aucun SettingsManager dans la scène : rien n'a été réinitialisé.");

        if (confirmResetPanel != null)
            confirmResetPanel.SetActive(false);
        Focus(resetButton);
    }

    // Menu Root si indiqué, sinon le Canvas principal qui contient ce menu
    GameObject RacineMenu()
    {
        if (menuRoot != null) return menuRoot;

        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.rootCanvas.gameObject : null;
    }

    static void Focus(GameObject target)
    {
        if (target != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(target);
    }
}
