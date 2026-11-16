using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarteReponseUI : MonoBehaviour
{
    [Header("Éléments créés dans Unity")]
    public GameObject confirmationPanel;
    public Image emplacementCarte;
    public Image apercuCarteDeposee;
    public Button boutonOui;
    public Button boutonNon;
    public Button boutonSolution;
    public GameObject fondSelection;

    [Header("Disposition")]
    public Vector2 decalagePortraitSelection = new Vector2(-300f, 0f);

    UIClasseur classeur;
    Func<CarteEmotion, bool> surConfirmation;
    PortraitSlot portraitGauche;
    PortraitSlot portraitDroit;
    CarteEmotion carteSelectionnee;
    Sprite cadreEmplacement;
    TMP_Text texteConfirmation;
    Action apresSolution;
    string indicesDisponibles;
    string solutionDisponible;
    RectTransform panneauConfirmationRect;
    RectTransform texteConfirmationRect;
    Vector2 positionPanneauInitiale;
    Vector2 taillePanneauInitiale;
    Vector2 positionTexteInitiale;
    Vector2 positionOuiInitiale;
    Vector2 positionNonInitiale;
    Vector2 positionSolutionInitiale;
    Vector2 tailleOuiInitiale;
    Vector2 tailleNonInitiale;
    Vector2 tailleSolutionInitiale;
    Vector2 tailleTexteInitiale;
    float taillePoliceInitiale;

    const string MessageConfirmation = "Confirmer cette carte ?";
    const string MessageErreur = "Erreur, voulez-vous recommencer ?";
    const string MessageSansIndice = "Aucun indice n’est disponible pour cette question.";
    const string MessageSansSolution = "La solution n’est pas encore renseignée dans le JSON.";

    public void Configurer()
    {
        Texture2D textureCadre = Resources.Load<Texture2D>("CadreEmplacementCarte");
        if (textureCadre == null)
            Debug.LogError("CarteReponseUI : l'image CadreEmplacementCarte est introuvable dans Assets/Resources.");
        else
        {
            cadreEmplacement = Sprite.Create(
                textureCadre,
                new Rect(0f, 0f, textureCadre.width, textureCadre.height),
                new Vector2(0.5f, 0.5f));
            cadreEmplacement.name = "CadreEmplacementCarte_Runtime";
        }

        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (confirmationPanel != null)
        {
            panneauConfirmationRect = confirmationPanel.transform as RectTransform;
            if (panneauConfirmationRect != null)
            {
                positionPanneauInitiale = panneauConfirmationRect.anchoredPosition;
                taillePanneauInitiale = panneauConfirmationRect.sizeDelta;
            }
            texteConfirmation = confirmationPanel.GetComponentInChildren<TMP_Text>(true);
            if (texteConfirmation == null)
                Debug.LogError("CarteReponseUI : ajoute un texte TMP dans ConfirmationCarte.");
            else
            {
                texteConfirmationRect = texteConfirmation.rectTransform;
                positionTexteInitiale = texteConfirmationRect.anchoredPosition;
                tailleTexteInitiale = texteConfirmationRect.sizeDelta;
                taillePoliceInitiale = texteConfirmation.fontSize;
                texteConfirmation.textWrappingMode = TextWrappingModes.Normal;
            }
        }
        if (boutonOui != null)
        {
            RectTransform rect = boutonOui.transform as RectTransform;
            if (rect != null)
            {
                positionOuiInitiale = rect.anchoredPosition;
                tailleOuiInitiale = rect.sizeDelta;
            }
        }
        if (boutonNon != null)
        {
            RectTransform rect = boutonNon.transform as RectTransform;
            if (rect != null)
            {
                positionNonInitiale = rect.anchoredPosition;
                tailleNonInitiale = rect.sizeDelta;
            }
        }
        if (boutonSolution != null)
        {
            RectTransform rect = boutonSolution.transform as RectTransform;
            if (rect != null)
            {
                positionSolutionInitiale = rect.anchoredPosition;
                tailleSolutionInitiale = rect.sizeDelta;
            }
        }
        if (boutonSolution == null && boutonOui != null)
        {
            GameObject copie = Instantiate(boutonOui.gameObject, boutonOui.transform.parent);
            copie.name = "BoutonSolution";
            boutonSolution = copie.GetComponent<Button>();
            if (boutonSolution == null)
            {
                Debug.LogError("CarteReponseUI : impossible de créer le bouton Solution.");
                Destroy(copie);
            }
            else
            {
                boutonSolution.onClick.RemoveAllListeners();
                RectTransform rect = boutonSolution.transform as RectTransform;
                if (rect != null)
                {
                    positionSolutionInitiale = rect.anchoredPosition;
                    tailleSolutionInitiale = rect.sizeDelta;
                }
                boutonSolution.gameObject.SetActive(false);
            }
        }
        if (emplacementCarte != null) emplacementCarte.gameObject.SetActive(false);
        if (apercuCarteDeposee != null)
        {
            apercuCarteDeposee.sprite = null;
            apercuCarteDeposee.raycastTarget = false;
            apercuCarteDeposee.gameObject.SetActive(false);
        }
        if (emplacementCarte != null && cadreEmplacement != null)
        {
            emplacementCarte.sprite = cadreEmplacement;
            emplacementCarte.type = Image.Type.Simple;
            emplacementCarte.preserveAspect = true;
        }
        if (fondSelection != null) fondSelection.SetActive(false);
        if (emplacementCarte != null && emplacementCarte.GetComponent<CarteDropTarget>() == null)
            emplacementCarte.gameObject.AddComponent<CarteDropTarget>();

        ConfigurerBoutonsConfirmation(EtatConfirmation.Validation);
    }

    void OnDestroy()
    {
        if (cadreEmplacement != null) Destroy(cadreEmplacement);
    }

    public void OuvrirSelection(
        UIClasseur classeur,
        Func<CarteEmotion, bool> onConfirmee,
        PortraitSlot gauche,
        PortraitSlot droite,
        string indices = "",
        string solution = "",
        Action apresSolution = null)
    {
        if (classeur == null || classeur.binderPanel == null)
        {
            Debug.LogError("CarteReponseUI : aucun panneau binderPanel valide n'est relié au UIClasseur.");
            return;
        }
        if (emplacementCarte == null)
        {
            Debug.LogError("CarteReponseUI : assigne l'Image EmplacementCarteChoisie pour recevoir la carte glissée.");
            return;
        }

        FermerSelection();
        this.classeur = classeur;
        surConfirmation = onConfirmee;
        indicesDisponibles = indices;
        solutionDisponible = solution;
        this.apresSolution = apresSolution;
        portraitGauche = gauche;
        portraitDroit = droite;

        if (fondSelection != null) fondSelection.SetActive(true);
        if (portraitGauche != null) portraitGauche.SetLayoutOffset(decalagePortraitSelection);
        if (portraitDroit != null) portraitDroit.SetLayoutOffset(decalagePortraitSelection);
        classeur.Ouvrir(true, AfficherConfirmation);

        if (confirmationPanel == null || emplacementCarte == null
            || boutonOui == null || boutonNon == null)
        {
            Debug.LogError("CarteReponseUI : le carnet est ouvert, mais il manque ConfirmationCarte, EmplacementCarteChoisie, BoutonOui ou BoutonNon dans DialogueUI.");
        }
    }

    public void OuvrirSelectionCompacte(
        UIClasseur classeur,
        Func<CarteEmotion, bool> onConfirmee,
        PortraitSlot gauche,
        PortraitSlot droite,
        string indices = "",
        string solution = "",
        Action apresSolution = null)
    {
        if (classeur == null || classeur.binderPanel == null)
        {
            Debug.LogError("CarteReponseUI : aucun panneau binderPanel valide n'est relié au UIClasseur.");
            return;
        }
        if (emplacementCarte == null)
        {
            Debug.LogError("CarteReponseUI : assigne l'Image EmplacementCarteChoisie pour recevoir la carte glissée.");
            return;
        }

        FermerSelection();
        this.classeur = classeur;
        surConfirmation = onConfirmee;
        indicesDisponibles = indices;
        solutionDisponible = solution;
        this.apresSolution = apresSolution;
        portraitGauche = gauche;
        portraitDroit = droite;
        if (fondSelection != null) fondSelection.SetActive(false);
        if (portraitGauche != null) portraitGauche.SetLayoutOffset(decalagePortraitSelection);
        if (portraitDroit != null) portraitDroit.SetLayoutOffset(decalagePortraitSelection);
        if (cadreEmplacement != null) emplacementCarte.sprite = cadreEmplacement;
        if (apercuCarteDeposee != null)
        {
            apercuCarteDeposee.sprite = null;
            apercuCarteDeposee.gameObject.SetActive(false);
        }
        emplacementCarte.raycastTarget = true;
        emplacementCarte.gameObject.SetActive(true);
        CarteDropTarget cibleDepot = emplacementCarte.GetComponent<CarteDropTarget>();
        cibleDepot.Configurer(classeur);
        classeur.OuvrirSelectionCompacte(AfficherConfirmation);
    }

    public void FermerSelection()
    {
        CancelInvoke(nameof(ContinuerApresSolution));
        if (classeur != null) classeur.Fermer();
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (texteConfirmation != null) texteConfirmation.text = MessageConfirmation;
        ConfigurerBoutonsConfirmation(EtatConfirmation.Validation);
        if (emplacementCarte != null)
        {
            emplacementCarte.gameObject.SetActive(false);
        }
        if (apercuCarteDeposee != null)
        {
            apercuCarteDeposee.sprite = null;
            apercuCarteDeposee.gameObject.SetActive(false);
        }
        if (fondSelection != null) fondSelection.SetActive(false);
        if (portraitGauche != null) portraitGauche.SetLayoutOffset(Vector2.zero);
        if (portraitDroit != null) portraitDroit.SetLayoutOffset(Vector2.zero);

        classeur = null;
        surConfirmation = null;
        apresSolution = null;
        indicesDisponibles = null;
        solutionDisponible = null;
        portraitGauche = null;
        portraitDroit = null;
        carteSelectionnee = null;
    }

    void AfficherConfirmation(CarteEmotion carte)
    {
        if (confirmationPanel == null || emplacementCarte == null || apercuCarteDeposee == null
            || boutonOui == null || boutonNon == null)
        {
            Debug.LogError("CarteReponseUI : impossible de confirmer la carte, l'interface Oui/Non n'est pas configurée.");
            if (classeur != null) classeur.OuvrirSelectionCompacte(AfficherConfirmation);
            return;
        }

        if (carte == null || carte.illustration == null)
        {
            Debug.LogError("CarteReponseUI : la carte sélectionnée n'a pas d'illustration.");
            if (classeur != null) classeur.OuvrirSelectionCompacte(AfficherConfirmation);
            return;
        }

        carteSelectionnee = carte;
        if (texteConfirmation != null) texteConfirmation.text = MessageConfirmation;
        ConfigurerBoutonsConfirmation(EtatConfirmation.Validation);
        apercuCarteDeposee.sprite = carte.illustration;
        apercuCarteDeposee.gameObject.SetActive(true);
        emplacementCarte.gameObject.SetActive(true);
        confirmationPanel.SetActive(true);
    }

    void Confirmer()
    {
        if (carteSelectionnee == null || surConfirmation == null)
        {
            Debug.LogError("CarteReponseUI : aucune carte à confirmer.");
            return;
        }

        CarteEmotion carte = carteSelectionnee;
        Func<CarteEmotion, bool> callback = surConfirmation;
        if (callback(carte))
        {
            FermerSelection();
            return;
        }

        carteSelectionnee = null;
        AfficherErreur();
    }

    void AfficherErreur()
    {
        if (texteConfirmation != null) texteConfirmation.text = MessageErreur;
        if (emplacementCarte != null) emplacementCarte.gameObject.SetActive(false);
        if (apercuCarteDeposee != null) apercuCarteDeposee.gameObject.SetActive(false);
        ConfigurerBoutonsConfirmation(EtatConfirmation.ChoixErreur);
        confirmationPanel.SetActive(true);
    }

    void Annuler()
    {
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (emplacementCarte != null)
        {
            if (cadreEmplacement != null) emplacementCarte.sprite = cadreEmplacement;
            emplacementCarte.gameObject.SetActive(true);
        }
        if (apercuCarteDeposee != null)
        {
            apercuCarteDeposee.sprite = null;
            apercuCarteDeposee.gameObject.SetActive(false);
        }
        carteSelectionnee = null;
        if (classeur != null) classeur.OuvrirSelectionCompacte(AfficherConfirmation);
    }

    enum EtatConfirmation { Validation, ChoixErreur, Indice, Solution }

    void ConfigurerBoutonsConfirmation(EtatConfirmation etat)
    {
        bool choixErreur = etat == EtatConfirmation.ChoixErreur;
        bool validation = etat == EtatConfirmation.Validation;
        AjusterDispositionConfirmation(validation, choixErreur);

        if (boutonOui != null)
        {
            boutonOui.onClick.RemoveAllListeners();
            boutonOui.onClick.AddListener(validation ? Confirmer
                : etat == EtatConfirmation.Indice ? Recommencer
                : etat == EtatConfirmation.Solution ? ContinuerApresSolution
                : Recommencer);
            boutonOui.gameObject.SetActive(true);
            DefinirTexteBouton(boutonOui, validation ? "Oui"
                : etat == EtatConfirmation.Solution ? "Continuer"
                : "Réessayer");
        }

        if (boutonOui != null)
        {
            RectTransform rect = boutonOui.transform as RectTransform;
            if (rect != null)
            {
                rect.anchoredPosition = validation ? positionOuiInitiale
                    : new Vector2(choixErreur ? -150f : 0f, PositionBoutonsY());
                rect.sizeDelta = validation ? tailleOuiInitiale : new Vector2(132f, 40f);
            }
        }

        if (boutonNon != null)
        {
            boutonNon.onClick.RemoveAllListeners();
            boutonNon.onClick.AddListener(validation ? Annuler : AfficherIndices);
            boutonNon.gameObject.SetActive(validation || choixErreur);
            DefinirTexteBouton(boutonNon, validation ? "Non" : "Indice");
            RectTransform rect = boutonNon.transform as RectTransform;
            if (rect != null)
            {
                rect.anchoredPosition = validation ? positionNonInitiale
                    : new Vector2(0f, PositionBoutonsY());
                rect.sizeDelta = validation ? tailleNonInitiale : new Vector2(132f, 40f);
            }
        }

        if (boutonSolution != null)
        {
            boutonSolution.onClick.RemoveAllListeners();
            if (choixErreur) boutonSolution.onClick.AddListener(AfficherSolution);
            boutonSolution.gameObject.SetActive(choixErreur);
            DefinirTexteBouton(boutonSolution, "Solution");
            RectTransform rect = boutonSolution.transform as RectTransform;
            if (rect != null)
            {
                rect.anchoredPosition = choixErreur
                    ? new Vector2(150f, PositionBoutonsY())
                    : positionSolutionInitiale;
                rect.sizeDelta = choixErreur ? new Vector2(132f, 40f) : tailleSolutionInitiale;
            }
        }
    }

    void AjusterDispositionConfirmation(bool validation, bool choixErreur)
    {
        if (panneauConfirmationRect == null || texteConfirmationRect == null) return;
        if (validation)
        {
            panneauConfirmationRect.anchoredPosition = positionPanneauInitiale;
            panneauConfirmationRect.sizeDelta = taillePanneauInitiale;
            texteConfirmationRect.anchoredPosition = positionTexteInitiale;
            texteConfirmationRect.sizeDelta = tailleTexteInitiale;
            texteConfirmation.fontSize = taillePoliceInitiale;
            return;
        }

        Canvas canvas = confirmationPanel.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        float largeurCanvas = canvasRect != null ? canvasRect.rect.width : Screen.width;
        float hauteurCanvas = canvasRect != null ? canvasRect.rect.height : Screen.height;
        float largeurPanel = Mathf.Min(560f, largeurCanvas - 40f);
        float largeurTexte = Mathf.Max(100f, largeurPanel - 48f);
        float hauteurTexteMax = Mathf.Max(80f, hauteurCanvas - 180f);

        texteConfirmation.textWrappingMode = TextWrappingModes.Normal;
        texteConfirmation.fontSize = taillePoliceInitiale;
        Vector2 taillePreferee = texteConfirmation.GetPreferredValues(
            texteConfirmation.text, largeurTexte, Mathf.Infinity);
        while (taillePreferee.y > hauteurTexteMax && texteConfirmation.fontSize > 12f)
        {
            texteConfirmation.fontSize = Mathf.Max(12f, texteConfirmation.fontSize - 1f);
            taillePreferee = texteConfirmation.GetPreferredValues(
                texteConfirmation.text, largeurTexte, Mathf.Infinity);
        }

        float hauteurTexte = Mathf.Min(taillePreferee.y, hauteurTexteMax);
        const float margeHaute = 24f;
        const float espaceBoutons = 24f;
        const float hauteurBouton = 40f;
        const float margeBasse = 20f;
        float hauteurPanel = hauteurTexte + margeHaute + espaceBoutons + hauteurBouton + margeBasse;

        panneauConfirmationRect.anchoredPosition = Vector2.zero;
        panneauConfirmationRect.sizeDelta = new Vector2(largeurPanel, hauteurPanel);
        texteConfirmationRect.sizeDelta = new Vector2(largeurTexte, hauteurTexte);
        texteConfirmationRect.anchoredPosition = new Vector2(
            0f, hauteurPanel * 0.5f - margeHaute - hauteurTexte * 0.5f);
    }

    float PositionBoutonsY()
    {
        return -texteConfirmationRect.rect.height * 0.5f - 24f - 20f;
    }

    void AfficherIndices()
    {
        if (texteConfirmation != null)
            texteConfirmation.text = string.IsNullOrWhiteSpace(indicesDisponibles)
                ? MessageSansIndice
                : indicesDisponibles;
        ConfigurerBoutonsConfirmation(EtatConfirmation.Indice);
    }

    void AfficherSolution()
    {
        string texte = string.IsNullOrWhiteSpace(solutionDisponible)
            ? MessageSansSolution
            : solutionDisponible;
        if (texteConfirmation != null) texteConfirmation.text = texte;
        ConfigurerBoutonsConfirmation(EtatConfirmation.Solution);
        Invoke(nameof(ContinuerApresSolution), Mathf.Max(3f, texte.Length / 15f));
    }

    void Recommencer()
    {
        UIClasseur classeurActuel = classeur;
        Func<CarteEmotion, bool> callback = surConfirmation;
        PortraitSlot gauche = portraitGauche;
        PortraitSlot droite = portraitDroit;
        string indices = indicesDisponibles;
        string solution = solutionDisponible;
        Action continuer = apresSolution;
        if (classeurActuel == null || callback == null)
        {
            Debug.LogError("CarteReponseUI : impossible de recommencer, la sélection de carte est incomplète.");
            FermerSelection();
            return;
        }

        FermerSelection();
        OuvrirSelectionCompacte(classeurActuel, callback, gauche, droite, indices, solution, continuer);
    }

    void ContinuerApresSolution()
    {
        CancelInvoke(nameof(ContinuerApresSolution));
        Action callback = apresSolution;
        FermerSelection();
        if (callback == null)
        {
            Debug.LogError("CarteReponseUI : aucune action n'est configurée après l'affichage de la solution.");
            return;
        }
        callback();
    }

    static void DefinirTexteBouton(Button bouton, string texte)
    {
        TMP_Text etiquette = bouton.GetComponentInChildren<TMP_Text>(true);
        if (etiquette == null)
        {
            Debug.LogError("CarteReponseUI : le bouton " + bouton.name + " n'a pas de texte TMP.");
            return;
        }
        etiquette.text = texte;
    }
}
