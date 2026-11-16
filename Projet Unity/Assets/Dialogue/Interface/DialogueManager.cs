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

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        IsActive = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        AfficherElements(false);

        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);
        CacherChoix();
    }

    void OnDestroy()
    {
        if (Instance == this) { Instance = null; IsActive = false; }
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
            if (portraitDroit != null) portraitDroit.Hide();
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

        if (typingRoutine != null) StopCoroutine(typingRoutine);
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

        float affiches = 0f;
        while (affiches < total)
        {
            affiches += typingSpeed > 0f ? Time.unscaledDeltaTime / typingSpeed : total;
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

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer);
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
        if (typingRoutine != null) { StopCoroutine(typingRoutine); typingRoutine = null; }
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

    /// <summary>Fin de toute la conversation : on retire aussi les sprites.</summary>
    public void FinConversation()
    {
        Masquer();
        AfficherElements(false);
        if (portraitGauche != null) portraitGauche.Hide();
        if (portraitDroit != null) portraitDroit.Hide();
    }
}
