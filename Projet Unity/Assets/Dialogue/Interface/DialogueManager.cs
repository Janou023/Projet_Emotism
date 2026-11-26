using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Affichage des dialogues : boîte de texte (machine à écrire), sprites, historique, boutons de choix.
/// UML : Demarrer(List&lt;Dialogue&gt;), LigneSuivante(), EstTermine().
/// Contrôles : Espace / Entrée / clic gauche = continuer (ou finir d'écrire la ligne).
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    /// <summary>Vrai pendant un dialogue ou une question : sert à bloquer le joueur.</summary>
    public static bool IsActive { get; private set; }

    [Header("Boîte de texte")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI textNom;
    public GameObject pastilleNom;           // optionnel : l'objet NomFond (caché quand il n'y a pas de nom)
    public TextMeshProUGUI textDialogue;
    public GameObject continueIcon;          // optionnel

    [Header("Sprites")]
    public PortraitSlot portraitGauche;
    public PortraitSlot portraitDroit;
    public Sprite defaultPlayerSprite;      // conservé pour ta scène
    public Sprite defaultNPCColorSprite;    // sprite affiché si le personnage n'a pas de portrait
    public Sprite portraitPenseeJoueur;

    [Header("Historique (optionnel)")]
    public DialogueHistory dialogueHistory;
    public string nomJoueur = "Moi";         // nom affiché pour les pensées

    [Header("Interface liée au dialogue")]
    [Tooltip("Affichés pendant un dialogue, cachés à la fin : Cadre, boutons Hist. / Carte...")]
    public GameObject[] elementsAvecDialogue;
    [Tooltip("Tant qu'un de ces panneaux est ouvert (classeur...), Espace/Entrée n'avancent pas le texte.")]
    public GameObject[] panneauxBloquants;

    [Header("Choix de réponse")]
    public RectTransform choicesContainer;
    public TextMeshProUGUI questionText;
    public Button[] choiceButtons;
    Canvas canvasChoix;
    Canvas canvasInvite;
    RectTransform dialoguePanelRect;
    Vector2 tailleDialogueInitiale;

    [Header("Réglages")]
    public float typingSpeed = 0.03f;       // secondes par caractère (0 = instantané)
    public bool avancerAuClic = false;      // laisse décoché si ta boîte a déjà un bouton qui appelle LigneSuivante()

    [Header("Événements")]
    public UnityEvent onDialogueStart = new UnityEvent();
    public UnityEvent onDialogueEnd = new UnityEvent();

    List<Dialogue> lignes;
    int index;
    bool typing;
    int startFrame;
    Coroutine typingRoutine;
    Action surFin;
    Action<int> surChoix;
    readonly Dictionary<TMP_Text, (float size, float spacing)> baseStyles =
        new Dictionary<TMP_Text, (float size, float spacing)>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        IsActive = false;
        if (dialoguePanel != null)
        {
            dialoguePanelRect = dialoguePanel.GetComponent<RectTransform>();
            if (dialoguePanelRect != null) tailleDialogueInitiale = dialoguePanelRect.sizeDelta;
        }
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        AfficherElements(false);

        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);
        CacherChoix();
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged += AppliquerReglages;
            AppliquerReglages();
        }
    }

    void OnEnable()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged -= AppliquerReglages;
            SettingsManager.Instance.OnSettingsChanged += AppliquerReglages;
            AppliquerReglages();
        }
    }

    void OnDestroy()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged -= AppliquerReglages;
        if (Instance == this) { Instance = null; IsActive = false; }
    }

    void AppliquerReglages()
    {
        if (SettingsManager.Instance == null) return;
        var reglages = SettingsManager.Instance.Data;

        AjouterStyle(textDialogue);
        AjouterStyle(textNom);
        AjouterStyle(questionText);
        if (choiceButtons != null)
            foreach (Button button in choiceButtons)
                if (button != null)
                    foreach (TMP_Text label in button.GetComponentsInChildren<TMP_Text>(true))
                        AjouterStyle(label);

        foreach (var style in baseStyles)
        {
            if (style.Key == null) continue;
            style.Key.fontSize = style.Value.size * AccessibleText.ScaleFor(reglages.textSizeLevel);
            style.Key.lineSpacing = style.Value.spacing + reglages.lineSpacing * 8f;
        }

        if (reglages.instantText && typing)
            FinishTyping();
    }

    void AjouterStyle(TMP_Text label)
    {
        if (label != null && !baseStyles.ContainsKey(label))
            baseStyles.Add(label, (label.fontSize, label.lineSpacing));
    }

    // ---------------- Dialogue ----------------

    public void Demarrer(List<Dialogue> liste, Action onFini = null)
    {
        if (liste == null || liste.Count == 0)
        {
            if (onFini != null) onFini();
            return;
        }

        Masquer();
        lignes = liste;
        index = 0;
        surFin = onFini;
        IsActive = true;
        startFrame = Time.frameCount;      // le clic qui a lancé le dialogue ne saute pas la 1re ligne

        AppliquerReglages();
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        AfficherElements(true);
        onDialogueStart.Invoke();
        AfficherLigne();
    }

    public void LigneSuivante()
    {
        if (lignes == null) return;
        if (typing) { FinishTyping(); return; }   // 1er appel : finit d'écrire la ligne
        index++;
        if (index >= lignes.Count) FinDesLignes();
        else AfficherLigne();
    }

    public void DisplayNextSentence() { LigneSuivante(); }
    public void FermerDialogue() { FinConversation(); }

    public bool EstTermine()
    {
        return lignes == null || index >= lignes.Count;
    }

    void Update()
    {
        if (lignes == null || Time.frameCount == startFrame) return;
        if (PauseMenu.EstEnPause) return;   // Espace / Entrée n'avancent pas le dialogue pendant la pause
        if (dialogueHistory != null && dialogueHistory.IsOpen) return;   // on lit l'historique : on n'avance pas
        if (panneauxBloquants != null)
            foreach (GameObject g in panneauxBloquants)
                if (g != null && g.activeInHierarchy) return;

        bool pressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
                       || Input.GetKeyDown(KeyCode.KeypadEnter)
                       || (avancerAuClic && Input.GetMouseButtonDown(0));
        if (!pressed) return;

        LigneSuivante();
    }

    void AfficherLigne()
    {
        Dialogue d = lignes[index];
        bool pensee = d.pensee || d.personnage == null;

        // Pensée du joueur : on affiche "Moi" comme nom
        AfficherNom(true, pensee ? nomJoueur : d.personnage.nom);
        textDialogue.fontStyle = pensee ? FontStyles.Italic : FontStyles.Normal;

        if (pensee)
        {
            if (portraitGauche != null) portraitGauche.Hide();
            if (portraitDroit != null)
            {
                if (portraitPenseeJoueur != null) portraitDroit.Show(portraitPenseeJoueur, true);
                else portraitDroit.Hide();
            }
        }
        else
        {
            bool gauche = d.personnage.cote == PortraitSide.Left;
            PortraitSlot slot  = gauche ? portraitGauche : portraitDroit;
            PortraitSlot autre = gauche ? portraitDroit : portraitGauche;
            Sprite sprite = d.personnage.GetPortrait(d.expression, d.utiliserExpression);
            if (sprite == null) sprite = defaultNPCColorSprite;
            if (slot != null) slot.Show(sprite, true);
            if (autre != null) autre.Hide();
        }

        if (dialogueHistory != null)
            dialogueHistory.Add(pensee ? nomJoueur : d.personnage.nom, pensee ? "<i>" + d.texte + "</i>" : d.texte);

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            if (SoundsManager.Instance != null)
                SoundsManager.Instance.StopTypingSound();
        }
        typingRoutine = StartCoroutine(Ecrire(d.texte));
    }

    IEnumerator Ecrire(string texte)
    {
        typing = true;
        if (continueIcon != null) continueIcon.SetActive(false);

        textDialogue.text = texte;
        textDialogue.maxVisibleCharacters = 0;
        textDialogue.ForceMeshUpdate();
        int total = textDialogue.textInfo.characterCount;
        AppliquerReglages();
        if (SettingsManager.Instance != null && SettingsManager.Instance.Data.instantText)
        {
            FinishTyping();
            yield break;
        }

        if (total > 0 && SoundsManager.Instance != null)
            SoundsManager.Instance.StartTypingSound();

        float affiches = 0f;
        while (affiches < total)
        {
            if (PauseMenu.EstEnPause) { yield return null; continue; }   // le texte cesse de s'écrire pendant la pause
            float vitesse = typingSpeed;
            if (SettingsManager.Instance != null)
            {
                float facteur = Mathf.Lerp(2f, 0.1f, SettingsManager.Instance.Data.textSpeed);
                vitesse *= facteur;
                if (SettingsManager.Instance.Data.instantText)
                {
                    FinishTyping();
                    yield break;
                }
            }
            affiches += vitesse > 0f ? Time.unscaledDeltaTime / vitesse : total;
            textDialogue.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(affiches));
            yield return null;
        }
        FinishTyping();
    }

    void FinishTyping()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = null;
        typing = false;
        if (SoundsManager.Instance != null)
            SoundsManager.Instance.StopTypingSound();
        textDialogue.maxVisibleCharacters = int.MaxValue;
        if (continueIcon != null) continueIcon.SetActive(true);
    }

    void FinDesLignes()
    {
        Action cb = surFin;
        Masquer();                       // les sprites restent affichés (FinConversation les retire)
        onDialogueEnd.Invoke();
        if (cb != null) cb();
    }

    // ---------------- Question / choix ----------------

    /// <summary>Affiche un texte fixe dans la boîte (sans avancer) : question en attente d'une réponse.</summary>
    public void AfficherInvite(string texte)
    {
        Masquer();
        MettreInviteAuPremierPlan();
        AppliquerReglages();
        IsActive = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(!string.IsNullOrEmpty(texte));
        if (textDialogue != null) textDialogue.gameObject.SetActive(true);
        AfficherNom(false, "");
        AfficherElements(true);
        textDialogue.fontStyle = FontStyles.Italic;
        textDialogue.text = texte ?? "";
        textDialogue.maxVisibleCharacters = int.MaxValue;
        if (dialogueHistory != null && !string.IsNullOrEmpty(texte))
            dialogueHistory.Add("", texte);
    }

    void MettreInviteAuPremierPlan()
    {
        if (dialoguePanel == null) return;
        canvasInvite = dialoguePanel.GetComponent<Canvas>();
        if (canvasInvite == null) canvasInvite = dialoguePanel.AddComponent<Canvas>();
        canvasInvite.overrideSorting = true;
        canvasInvite.sortingOrder = 25;
        if (dialoguePanel.GetComponent<GraphicRaycaster>() == null)
            dialoguePanel.AddComponent<GraphicRaycaster>();
    }

    public void AfficherChoix(string invite, IList<string> textes, Action<int> onChoix)
    {
        Masquer();
        surChoix = onChoix;

        if (choicesContainer == null || questionText == null ||
            choiceButtons == null || choiceButtons.Length < textes.Count)
        {
            Debug.LogError("DialogueManager : assigne ChoiceContainer, QuestionText et un bouton par choix possible.");
            return;
        }

        IsActive = true;
        startFrame = Time.frameCount;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        AfficherElements(true);
        AfficherNom(false, "");
        if (textDialogue != null) textDialogue.gameObject.SetActive(false);
        bool afficherQuestion = !string.IsNullOrWhiteSpace(invite);
        questionText.text = invite ?? "";
        questionText.gameObject.SetActive(afficherQuestion);
        if (dialogueHistory != null && !string.IsNullOrEmpty(invite))
            dialogueHistory.Add("", invite);

        choicesContainer.gameObject.SetActive(true);
        MettreChoixAuPremierPlan();
        ConfigurerDispositionChoix(textes.Count);
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            Button button = choiceButtons[i];
            if (button == null)
            {
                Debug.LogError("DialogueManager : un bouton de choix n'est pas assigné.");
                continue;
            }

            bool actif = i < textes.Count;
            button.gameObject.SetActive(actif);
            if (!actif) continue;

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
            {
                Debug.LogError("DialogueManager : le bouton " + button.name + " n'a pas de texte TMP.");
                continue;
            }

            label.text = textes[i];
            button.onClick.RemoveAllListeners();
            int index = i;
            button.onClick.AddListener(() => Choisir(index));
        }

        AppliquerReglages();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer);
    }

    void ConfigurerDispositionChoix(int nombreChoix)
    {
        if (choicesContainer == null || choiceButtons == null || choiceButtons.Length == 0) return;
        RectTransform panneau = choicesContainer.parent as RectTransform;
        RectTransform reponses = choiceButtons[0] != null
            ? choiceButtons[0].transform.parent as RectTransform
            : null;
        if (panneau == null || reponses == null) return;

        VerticalLayoutGroup dispositionGenerale = choicesContainer.GetComponent<VerticalLayoutGroup>();
        if (dispositionGenerale != null) dispositionGenerale.enabled = false;

        float hauteurBouton = 80f;
        float espacementBoutons = 8f;
        float hauteurSouhaitee = nombreChoix * hauteurBouton
            + Mathf.Max(0, nombreChoix - 1) * espacementBoutons + 40f;
        RectTransform canvasRect = dialoguePanelRect != null
            ? dialoguePanelRect.root as RectTransform
            : null;
        float hauteurMax = canvasRect != null ? canvasRect.rect.height * 0.52f : hauteurSouhaitee;
        float hauteurPanneau = Mathf.Min(Mathf.Max(panneau.rect.height, hauteurSouhaitee), hauteurMax);
        if (dialoguePanelRect != null)
            dialoguePanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, hauteurPanneau);

        float largeurPanneau = panneau.rect.width;
        float largeurQuestion = largeurPanneau * 0.36f;
        float largeurReponses = Mathf.Min(760f, largeurPanneau * 0.60f);

        RectTransform questionRect = questionText != null ? questionText.rectTransform : null;
        if (questionRect != null)
        {
            questionRect.anchorMin = new Vector2(0f, 1f);
            questionRect.anchorMax = new Vector2(0f, 1f);
            questionRect.pivot = new Vector2(0f, 1f);
            questionRect.anchoredPosition = new Vector2(24f, -20f);
            questionRect.sizeDelta = new Vector2(largeurQuestion, Mathf.Min(100f, hauteurPanneau * 0.42f));
            questionText.textWrappingMode = TextWrappingModes.Normal;
            questionText.enableAutoSizing = true;
            questionText.fontSizeMin = 18f;
            questionText.fontSizeMax = 25f;
            questionText.alignment = TextAlignmentOptions.TopLeft;
        }

        reponses.anchorMin = new Vector2(1f, 0f);
        reponses.anchorMax = new Vector2(1f, 0f);
        reponses.pivot = new Vector2(1f, 0f);
        reponses.anchoredPosition = new Vector2(-18f, 10f);
        reponses.sizeDelta = new Vector2(largeurReponses, Mathf.Max(0f, hauteurPanneau - 36f));

        VerticalLayoutGroup dispositionReponses = reponses.GetComponent<VerticalLayoutGroup>();
        if (dispositionReponses != null)
        {
            dispositionReponses.childAlignment = TextAnchor.MiddleCenter;
            dispositionReponses.childForceExpandWidth = false;
            dispositionReponses.childControlWidth = true;
            dispositionReponses.spacing = espacementBoutons;
        }

        foreach (Button button in choiceButtons)
        {
            if (button == null) continue;
            LayoutElement taille = button.GetComponent<LayoutElement>();
            if (taille == null) taille = button.gameObject.AddComponent<LayoutElement>();
            taille.preferredWidth = largeurReponses;
            taille.preferredHeight = hauteurBouton;
            taille.flexibleWidth = 0f;

            TMP_Text etiquette = button.GetComponentInChildren<TMP_Text>(true);
            if (etiquette == null) continue;
            etiquette.textWrappingMode = TextWrappingModes.Normal;
            etiquette.enableAutoSizing = false;
            etiquette.fontSize = 28f;
            baseStyles[etiquette] = (28f, etiquette.lineSpacing);
            etiquette.alignment = TextAlignmentOptions.Center;
        }
    }

    void MettreChoixAuPremierPlan()
    {
        if (choicesContainer == null) return;

        canvasChoix = choicesContainer.GetComponent<Canvas>();
        if (canvasChoix == null) canvasChoix = choicesContainer.gameObject.AddComponent<Canvas>();
        canvasChoix.overrideSorting = true;
        canvasChoix.sortingOrder = 30;

        if (choicesContainer.GetComponent<GraphicRaycaster>() == null)
            choicesContainer.gameObject.AddComponent<GraphicRaycaster>();
    }

    void Choisir(int i)
    {
        Action<int> cb = surChoix;
        Masquer();
        if (cb != null) cb(i);
    }

    void CacherChoix()
    {
        if (choiceButtons == null) return;
        foreach (Button button in choiceButtons)
            if (button != null) button.gameObject.SetActive(false);
    }

    // ---------------- Divers ----------------

    public void AjouterAuHistorique(string nom, string texte)
    {
        if (dialogueHistory != null) dialogueHistory.Add(nom, texte);
    }

    void AfficherNom(bool afficher, string nom)
    {
        if (textNom == null) return;
        if (afficher) textNom.text = nom;
        GameObject cible = pastilleNom != null ? pastilleNom : textNom.gameObject;
        cible.SetActive(afficher);
    }

    void AfficherElements(bool afficher)
    {
        if (elementsAvecDialogue == null) return;
        foreach (GameObject g in elementsAvecDialogue)
            if (g != null) g.SetActive(afficher);
    }

    /// <summary>Cache la boîte, les choix et la question (les sprites restent).</summary>
    public void Masquer()
    {
        RestaurerTailleDialogue();
        if (typingRoutine != null) { StopCoroutine(typingRoutine); typingRoutine = null; }
        if (SoundsManager.Instance != null)
            SoundsManager.Instance.StopTypingSound();
        typing = false;
        lignes = null;
        surFin = null;
        surChoix = null;
        CacherChoix();
        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);
        if (questionText != null) questionText.gameObject.SetActive(false);
        if (textDialogue != null) textDialogue.gameObject.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (continueIcon != null) continueIcon.SetActive(false);
        IsActive = false;
    }

    void RestaurerTailleDialogue()
    {
        if (dialoguePanelRect != null)
            dialoguePanelRect.sizeDelta = tailleDialogueInitiale;
    }

    /// <summary>Fin de toute la conversation : on retire aussi les sprites.</summary>
    public void FinConversation()
    {
        Masquer();
        AfficherElements(false);
        if (portraitGauche != null) portraitGauche.Hide();
        if (portraitDroit != null) portraitDroit.Hide();
    }
}
