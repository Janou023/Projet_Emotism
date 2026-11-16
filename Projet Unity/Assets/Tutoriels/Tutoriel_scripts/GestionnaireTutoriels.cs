using System;
using System.Collections.Generic;
using UnityEngine;

public class GestionnaireTutoriels : MonoBehaviour
{
    private const string PrefixeCleSauvegarde = "TutorielVu_";
    private static readonly HashSet<string> tutorielsReinitialisesPourTest = new HashSet<string>();

    [SerializeField] private TutorielPopUP panneauTutoriel;
    [SerializeField] private ContenuTutoriel tutorielAuLancement;
    [SerializeField] private ContenuTutoriel aideCartes;
    [SerializeField] private ContenuTutoriel aideChoix;
    [SerializeField] private bool reinitialiserTutorielsPourTestAuDemarrage;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReinitialiserSessionDeTest()
    {
        tutorielsReinitialisesPourTest.Clear();
    }

    private void Awake()
    {
        if (!reinitialiserTutorielsPourTestAuDemarrage)
        {
            return;
        }

        bool preferencesModifiees = ReinitialiserStatutPourTest(tutorielAuLancement);
        preferencesModifiees |= ReinitialiserStatutPourTest(aideCartes);
        preferencesModifiees |= ReinitialiserStatutPourTest(aideChoix);
        if (preferencesModifiees)
        {
            PlayerPrefs.Save();
        }
    }

    private void Start()
    {
        if (tutorielAuLancement == null)
        {
            return;
        }

        Debug.Log("Demande du tutoriel au lancement : " + tutorielAuLancement.id, this);
        AfficherTutoriel(tutorielAuLancement);
    }

    public void AfficherTutoriel(ContenuTutoriel contenu)
    {
        AfficherTutoriel(contenu, null);
    }

    private bool AfficherTutoriel(ContenuTutoriel contenu, Action onClosed)
    {
        if (contenu == null)
        {
            Debug.LogWarning("Aucun contenu de tutoriel n'a ete fourni.", this);
            return false;
        }

        if (panneauTutoriel == null)
        {
            Debug.LogError("Assigne le panneau de tutoriel dans l'Inspector.", this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(contenu.id))
        {
            Debug.LogError("Chaque tutoriel doit avoir un identifiant unique.", contenu);
            return false;
        }

        string cleSauvegarde = PrefixeCleSauvegarde + contenu.id;
        if (PlayerPrefs.GetInt(cleSauvegarde, 0) == 1)
        {
            Debug.LogWarning("Tutoriel deja enregistre comme vu : " + contenu.id, this);
            return false;
        }

        Debug.Log("Contenu envoye au panneau - titre: '" + contenu.titre + "', texte: '" + contenu.texte + "'.", this);
        if (!panneauTutoriel.ShowPopUp(contenu, onClosed))
        {
            return false;
        }

        PlayerPrefs.SetInt(cleSauvegarde, 1);
        PlayerPrefs.Save();
        return true;
    }

    private static bool ReinitialiserStatutPourTest(ContenuTutoriel contenu)
    {
        if (contenu == null || string.IsNullOrWhiteSpace(contenu.id) ||
            !tutorielsReinitialisesPourTest.Add(contenu.id))
        {
            return false;
        }

        string cleSauvegarde = PrefixeCleSauvegarde + contenu.id;
        if (!PlayerPrefs.HasKey(cleSauvegarde))
        {
            return false;
        }

        PlayerPrefs.DeleteKey(cleSauvegarde);
        return true;
    }

    public void AfficherAideCartes()
    {
        AfficherTutoriel(aideCartes);
    }

    public bool AfficherAideCartes(Action onClosed)
    {
        return AfficherTutoriel(aideCartes, onClosed);
    }

    public void AfficherAideChoix()
    {
        AfficherTutoriel(aideChoix);
    }
}
