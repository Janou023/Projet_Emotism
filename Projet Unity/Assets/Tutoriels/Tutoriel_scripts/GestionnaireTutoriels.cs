using UnityEngine;

public class GestionnaireTutoriels : MonoBehaviour
{
    [SerializeField] private TutorielPopUP panneauTutoriel;
    [SerializeField] private ContenuTutoriel tutorielAuLancement;

    private void Start()
    {
        if (tutorielAuLancement == null)
        {
            Debug.LogWarning("Aucun tutoriel n'est assigne au lancement.", this);
            return;
        }

        Debug.Log("Demande du tutoriel au lancement : " + tutorielAuLancement.id, this);
        AfficherTutoriel(tutorielAuLancement);
    }

    public void AfficherTutoriel(ContenuTutoriel contenu)
    {
        if (contenu == null)
        {
            Debug.LogWarning("Aucun contenu de tutoriel n'a ete fourni.", this);
            return;
        }

        if (panneauTutoriel == null)
        {
            Debug.LogError("Assigne le panneau de tutoriel dans l'Inspector.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(contenu.id))
        {
            Debug.LogError("Chaque tutoriel doit avoir un identifiant unique.", contenu);
            return;
        }

        string cleSauvegarde = "TutorielVu_" + contenu.id;
        // if (PlayerPrefs.GetInt(cleSauvegarde, 0) == 1)
        // {
        //     Debug.LogWarning("Tutoriel deja enregistre comme vu : " + contenu.id, this);
        //     return;
        // }

        Debug.Log("Contenu envoye au panneau - titre: '" + contenu.titre + "', texte: '" + contenu.texte + "'.", this);
        if (!panneauTutoriel.ShowPopUp(contenu))
        {
            return;
        }

        PlayerPrefs.SetInt(cleSauvegarde, 1);
        PlayerPrefs.Save();
    }
}
