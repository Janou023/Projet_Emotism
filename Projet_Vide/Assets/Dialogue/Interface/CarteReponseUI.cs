using System;
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
    public GameObject fondSelection;

    [Header("Disposition")]
    public Vector2 decalagePortraitSelection = new Vector2(-300f, 0f);

    UIClasseur classeur;
    Action<CarteEmotion> surConfirmation;
    PortraitSlot portraitGauche;
    PortraitSlot portraitDroit;
    CarteEmotion carteSelectionnee;
    Sprite cadreEmplacement;

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

        if (boutonOui != null)
        {
            boutonOui.onClick.RemoveListener(Confirmer);
            boutonOui.onClick.AddListener(Confirmer);
        }
        if (boutonNon != null)
        {
            boutonNon.onClick.RemoveListener(Annuler);
            boutonNon.onClick.AddListener(Annuler);
        }
    }

    void OnDestroy()
    {
        if (cadreEmplacement != null) Destroy(cadreEmplacement);
    }

    public void OuvrirSelection(
        UIClasseur classeur,
        Action<CarteEmotion> onConfirmee,
        PortraitSlot gauche,
        PortraitSlot droite)
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
        Action<CarteEmotion> onConfirmee,
        PortraitSlot gauche,
        PortraitSlot droite)
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
        if (classeur != null) classeur.Fermer();
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
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
        Action<CarteEmotion> callback = surConfirmation;
        FermerSelection();
        callback(carte);
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
}
