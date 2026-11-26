using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TutorielPopUP : MonoBehaviour
{
    [SerializeField] private TMP_Text titreText;
    [SerializeField] private TMP_Text contenuText;
    [SerializeField, Min(0f)] private float dureeFondu = 0.25f;
    private Action onClosed;
    private CanvasGroup groupeCanvas;
    private Coroutine transition;

    public void Configurer(TMP_Text titre, TMP_Text contenu)
    {
        titreText = titre;
        contenuText = contenu;
    }

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
        if (transition != null) StopCoroutine(transition);
        groupeCanvas = ObtenirGroupeCanvas();
        gameObject.SetActive(true);
        groupeCanvas.alpha = 0f;
        groupeCanvas.interactable = false;
        groupeCanvas.blocksRaycasts = true;
        transition = StartCoroutine(AnimerFondu(1f, false));
        Debug.Log("ShowPopUp a active '" + gameObject.name + "' : activeSelf=" + gameObject.activeSelf + ", activeInHierarchy=" + gameObject.activeInHierarchy, this);
        return true;
    }

    public void ClosePopUp()
    {
        if (transition != null) StopCoroutine(transition);
        groupeCanvas = ObtenirGroupeCanvas();
        groupeCanvas.interactable = false;
        groupeCanvas.blocksRaycasts = true;
        transition = StartCoroutine(AnimerFondu(0f, true));
    }

    CanvasGroup ObtenirGroupeCanvas()
    {
        if (groupeCanvas == null)
        {
            groupeCanvas = GetComponent<CanvasGroup>();
            if (groupeCanvas == null) groupeCanvas = gameObject.AddComponent<CanvasGroup>();
        }
        return groupeCanvas;
    }

    IEnumerator AnimerFondu(float alphaCible, bool fermer)
    {
        float alphaDepart = groupeCanvas.alpha;
        float temps = 0f;
        float duree = Mathf.Max(0f, dureeFondu);
        while (temps < duree)
        {
            temps += Time.unscaledDeltaTime;
            float progression = Mathf.Clamp01(temps / duree);
            groupeCanvas.alpha = Mathf.Lerp(alphaDepart, alphaCible, Mathf.SmoothStep(0f, 1f, progression));
            yield return null;
        }

        groupeCanvas.alpha = alphaCible;
        transition = null;
        if (!fermer)
        {
            groupeCanvas.interactable = true;
            groupeCanvas.blocksRaycasts = true;
            yield break;
        }

        gameObject.SetActive(false);
        Action callback = onClosed;
        onClosed = null;
        if (callback != null)
            callback();
    }
}
    