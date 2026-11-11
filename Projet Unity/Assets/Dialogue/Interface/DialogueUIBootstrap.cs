using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogueUIBootstrap : MonoBehaviour
{
    public TextAsset dialoguesJson;
    [Header("Scène")]
    public Sprite fondScene;
    public Sprite portraitManon;
    public UIClasseur classeurPrefab;
    public string nomJoueur = "Moi";
    public Vector2 decalagePortraitSelection = new Vector2(-450f, 0f);

    DialogueManager dialogueManager;
    DefiController defiController;
    UIClasseur classeur;
    CarteReponseUI interfaceCarte;

    void Awake()
    {
        Canvas canvas = TrouverCanvas();
        if (canvas == null)
        {
            Debug.LogError("DialogueUIBootstrap : ajoute le Canvas et DialogueUI dans la scène.");
            return;
        }
        if (fondScene != null)
        {
            Image imageFond = Composant<Image>(canvas.transform, "Fond");
            if (imageFond == null)
                Debug.LogError("DialogueUIBootstrap : l'image UI nommée Fond est absente du Canvas.");
            else
                imageFond.sprite = fondScene;
        }
        if (canvas.GetComponent<GraphicRaycaster>() == null)
            Debug.LogError("DialogueUIBootstrap : le Canvas doit avoir un Graphic Raycaster.");
        if (FindAnyObjectByType<EventSystem>() == null)
            Debug.LogError("DialogueUIBootstrap : ajoute un EventSystem dans la scène.");

        Transform ui = Chercher(canvas.transform, "DialogueUI");
        if (ui == null)
        {
            Debug.LogError("DialogueUIBootstrap : DialogueUI est absent du Canvas.");
            return;
        }

        dialogueManager = ui.GetComponent<DialogueManager>();
        if (dialogueManager == null) dialogueManager = ui.gameObject.AddComponent<DialogueManager>();

        GameObject panel = Objet(ui, "DialoguePanel");
        TextMeshProUGUI texte = Composant<TextMeshProUGUI>(ui, "Texte");
        TextMeshProUGUI nom = Composant<TextMeshProUGUI>(ui, "Nom");
        dialogueManager.dialoguePanel = panel;
        dialogueManager.textDialogue = texte;
        dialogueManager.textNom = nom;
        if (panel == null || texte == null)
        {
            Debug.LogError("DialogueUIBootstrap : DialoguePanel et son texte TMP nommé Texte sont requis.");
            return;
        }
        dialogueManager.pastilleNom = Objet(ui, "NomFond");
        dialogueManager.dialogueHistory = ui.GetComponent<DialogueHistory>();
        dialogueManager.nomJoueur = nomJoueur;
        GameObject choicesObject = Objet(ui, "ChoiceContainer") ?? Objet(ui, "ChoicesContainer");
        Transform choices = choicesObject != null ? choicesObject.transform : null;
        Transform reponses = choices != null ? Chercher(choices, "Reponses") : null;
        dialogueManager.choicesContainer = choices as RectTransform;
        dialogueManager.questionText = Composant<TextMeshProUGUI>(choices, "QuestionText");
        dialogueManager.choiceButtons = reponses != null
            ? reponses.GetComponentsInChildren<Button>(true)
            : choices != null ? choices.GetComponentsInChildren<Button>(true) : new Button[0];
        if (choices == null || dialogueManager.questionText == null)
            Debug.LogError("DialogueUIBootstrap : ajoute ChoiceContainer avec un enfant TMP nommé QuestionText.");
        if (dialogueManager.choiceButtons.Length == 0)
            Debug.LogError("DialogueUIBootstrap : ajoute les boutons de réponse dans Reponses.");
        dialogueManager.portraitGauche = Slot(ui, "Portrait_Gauche");
        dialogueManager.portraitDroit = Slot(ui, "Portrait_Droit");
        dialogueManager.defaultNPCColorSprite = portraitManon;

        Button continuer = panel != null ? panel.GetComponent<Button>() : null;
        if (continuer != null)
        {
            continuer.onClick.RemoveAllListeners();
            continuer.onClick.AddListener(dialogueManager.LigneSuivante);
        }
        Button historique = Bouton(ui, "BtnHistorique");
        if (historique == null)
            Debug.LogError("DialogueUIBootstrap : bouton BtnHistorique absent de la scène.");
        if (dialogueManager.dialogueHistory == null)
            Debug.LogError("DialogueUIBootstrap : ajoute DialogueHistory à DialogueUI.");
        if (historique != null && dialogueManager.dialogueHistory != null)
        {
            Transform historyRoot = dialogueManager.dialogueHistory.historyPanel != null
                ? dialogueManager.dialogueHistory.historyPanel.transform
                : ui;
            dialogueManager.dialogueHistory.historyText =
                Composant<TextMeshProUGUI>(historyRoot, "HistoryText");
            if (dialogueManager.dialogueHistory.historyText == null)
                Debug.LogError("DialogueUIBootstrap : ajoute un TextMeshPro nommé HistoryText dans le Content de l'historique.");
            if (historique.onClick.GetPersistentEventCount() == 0)
                historique.onClick.AddListener(dialogueManager.dialogueHistory.Toggle);
        }

        if (panel != null) panel.SetActive(false);
        if (choices != null) choices.gameObject.SetActive(false);

        defiController = GetComponent<DefiController>();
        if (defiController == null) defiController = gameObject.AddComponent<DefiController>();
        defiController.dialoguesJson = dialoguesJson;
        defiController.dialogueManager = dialogueManager;
        defiController.personnages = new[]
        {
            new Personnage
            {
                nom = "Manon",
                cote = PortraitSide.Right,
                portraitNeutre = portraitManon
            }
        };

        classeur = FindAnyObjectByType<UIClasseur>();
        if (classeur == null && classeurPrefab != null)
            classeur = Instantiate(classeurPrefab, canvas.transform);
        if (classeur == null)
            Debug.LogError("DialogueUIBootstrap : aucun UIClasseur trouvé. Assigne le prefab Canvas du carnet dans 'Classeur Prefab'.");
        else if (classeur.binderPanel == null)
            Debug.LogError("DialogueUIBootstrap : le UIClasseur trouvé n'a pas de Binder Panel assigné.");
        else
            classeur.Fermer();
        defiController.classeur = classeur;
        interfaceCarte = GetComponent<CarteReponseUI>();
        if (interfaceCarte == null) interfaceCarte = gameObject.AddComponent<CarteReponseUI>();
        interfaceCarte.confirmationPanel = Objet(ui, "ConfirmationCarte");
        interfaceCarte.emplacementCarte = Composant<Image>(ui, "EmplacementCarteChoisie");
        interfaceCarte.apercuCarteDeposee = Composant<Image>(ui, "ApercuCarteDeposee");
        interfaceCarte.boutonOui = Bouton(ui, "BoutonOui");
        interfaceCarte.boutonNon = Bouton(ui, "BoutonNon");
        interfaceCarte.fondSelection = Objet(ui, "FondSelectionCarte");
        interfaceCarte.decalagePortraitSelection = decalagePortraitSelection;
        interfaceCarte.Configurer();
        defiController.interfaceCarte = interfaceCarte;
        if (interfaceCarte.confirmationPanel != null)
            AjouterBloqueur(interfaceCarte.confirmationPanel);
        ConfigurerClasseur(ui);
    }

    public void LancerDialogue()
    {
        LancerDialogue(dialoguesJson, null, true, null);
    }

    public void LancerDialogue(TextAsset json, Sprite fond, bool estDefi, string sceneRetour)
    {
        if (json == null)
        {
            Debug.LogError("DialogueUIBootstrap : assigne le fichier JSON du dialogue.");
            return;
        }

        if (fond != null)
        {
            Canvas canvas = TrouverCanvas();
            Image imageFond = Composant<Image>(canvas != null ? canvas.transform : null, "Fond");
            if (imageFond == null)
                Debug.LogError("DialogueUIBootstrap : l'image UI nommée Fond est absente du Canvas.");
            else
                imageFond.sprite = fond;
        }

        defiController.afficherAnimationVictoire = estDefi;
        if (!string.IsNullOrEmpty(sceneRetour))
            defiController.onDefiTermine.AddListener(() => RetournerScene(sceneRetour));
        defiController.Lancer(json, 0);
    }

    void RetournerScene(string sceneRetour)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneRetour))
        {
            Debug.LogError("DialogueUIBootstrap : scène de retour absente des scènes de build : " + sceneRetour);
            return;
        }

        if (FadeManager.Instance != null)
            FadeManager.Instance.LoadSceneWithFade(sceneRetour);
        else
            SceneManager.LoadScene(sceneRetour);
    }

    void ConfigurerClasseur(Transform ui)
    {
        Button bouton = Bouton(ui, "BtnClasseur");
        if (bouton == null)
        {
            Debug.LogError("DialogueUIBootstrap : bouton BtnClasseur absent de DialogueUI.");
            return;
        }

        bouton.onClick.RemoveAllListeners();
        if (classeur == null)
        {
            return;
        }

        bouton.onClick.AddListener(defiController.OuvrirClasseur);
        if (classeur.binderPanel != null)
            AjouterBloqueur(classeur.binderPanel);
    }

    void AjouterBloqueur(GameObject panneau)
    {
        List<GameObject> panneaux = new List<GameObject>();
        if (dialogueManager.panneauxBloquants != null)
            panneaux.AddRange(dialogueManager.panneauxBloquants);
        if (!panneaux.Contains(panneau)) panneaux.Add(panneau);
        dialogueManager.panneauxBloquants = panneaux.ToArray();
    }

    Canvas TrouverCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
            if (Chercher(canvas.transform, "DialogueUI") != null)
                return canvas;
        return null;
    }

    PortraitSlot Slot(Transform ui, string name)
    {
        Transform found = Chercher(ui, name);
        return found != null ? found.GetComponent<PortraitSlot>() : null;
    }

    GameObject Objet(Transform parent, string name)
    {
        Transform found = Chercher(parent, name);
        return found != null ? found.gameObject : null;
    }

    Button Bouton(Transform parent, string name)
    {
        Transform found = Chercher(parent, name);
        return found != null ? found.GetComponent<Button>() : null;
    }

    T Composant<T>(Transform parent, string name) where T : Component
    {
        Transform found = Chercher(parent, name);
        return found != null ? found.GetComponent<T>() : null;
    }

    Transform Chercher(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = Chercher(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
