using UnityEngine;

/// <summary>
/// OUTIL DE TEST TEMPORAIRE. Affiche deux boutons en haut à gauche de l'écran :
/// - donner une carte au joueur (comme le ferait le JSON avec "carte_obtenue") ;
/// - effacer la progression pour pouvoir recommencer le test.
/// À placer sur un objet vide de la scène « Dialogue ». À supprimer après les tests.
/// Ne s'affiche que dans l'éditeur et dans les builds de développement.
/// </summary>
public class TestRecompenseOutil : MonoBehaviour
{
    [Tooltip("Identifiant de la carte à donner (ex. colere).")]
    [SerializeField] string idCarte = "colere";

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 300, 40), "TEST : obtenir la carte « " + idCarte + " »"))
            DonnerCarte();

        if (GUI.Button(new Rect(10, 55, 300, 40), "TEST : effacer la progression"))
            EffacerProgression();
    }

    void DonnerCarte()
    {
        UIClasseur classeur = FindAnyObjectByType<UIClasseur>();
        CardRewardPopup popup = FindAnyObjectByType<CardRewardPopup>(FindObjectsInactive.Include);

        if (classeur == null)
        {
            Debug.LogWarning("[Test] Aucun UIClasseur dans la scène : le classeur est créé au lancement par le bootstrap.");
            return;
        }
        if (popup == null)
        {
            Debug.LogWarning("[Test] Aucun CardRewardPopup dans la scène : il faut créer PopupCarte.");
            return;
        }

        bool fenetreAffichee = RecompenseCarte.Accorder(classeur, popup, idCarte,
            () => Debug.Log("[Test] Fin de l'obtention : ici, le dialogue reprendrait."));

        if (!fenetreAffichee)
            Debug.Log("[Test] Pas de fenêtre : la carte est déjà obtenue, ou introuvable (voir les avertissements).");
    }

    void EffacerProgression()
    {
        if (ProgressManager.Instance == null)
        {
            Debug.LogWarning("[Test] Aucun ProgressManager dans la scène.");
            return;
        }

        ProgressManager.Instance.ResetProgress();
        Debug.Log("[Test] Progression effacée. Relance la scène pour retrouver la carte de départ.");
    }
#endif
}
