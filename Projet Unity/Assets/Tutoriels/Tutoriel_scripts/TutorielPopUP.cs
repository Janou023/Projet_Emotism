using System;
using TMPro;
using UnityEngine;

public class TutorielPopUP : MonoBehaviour
{
    [SerializeField] private TMP_Text titreText;
    [SerializeField] private TMP_Text contenuText;
    private Action onClosed;

    public bool ShowPopUp(ContenuTutoriel contenu)
    {
        return ShowPopUp(contenu, null);
    }

    public bool ShowPopUp(ContenuTutoriel contenu, Action onClosed)
    {
        if (contenu == null)
        {
            Debug.LogWarning("Aucun contenu de tutoriel n'a ete fourni.", this);
            return false;
        }

        if (titreText == null || contenuText == null)
        {
            Debug.LogError("Assigne les champs TMP du panneau dans l'Inspector.", this);
            return false;
        }

        Debug.Log("Textes TMP cibles : " + titreText.name + " et " + contenuText.name, this);
        titreText.text = contenu.titre;
        contenuText.text = contenu.texte;
        this.onClosed = onClosed;
        gameObject.SetActive(true);
        Debug.Log("ShowPopUp a active '" + gameObject.name + "' : activeSelf=" + gameObject.activeSelf + ", activeInHierarchy=" + gameObject.activeInHierarchy, this);
        return true;
    }

    public void ClosePopUp()
    {
        gameObject.SetActive(false);
        Action callback = onClosed;
        onClosed = null;
        if (callback != null)
            callback();
    }
}
    