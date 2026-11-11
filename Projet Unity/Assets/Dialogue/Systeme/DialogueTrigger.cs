using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// À mettre sur un PNJ / objet avec un Collider2D coché "Is Trigger".
/// Le joueur s'approche et appuie sur la touche d'interaction pour lancer le dialogue.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Source des Données")]
    public DefiController defi;
    public TextAsset jsonDialogue;            // Glisse 'dialogues_tram.json' ici (sinon celui du DefiController)
    public bool ouvrirSceneDialogue;
    public string sceneDialogue = "Dialogue";
    public Sprite fondDialogue;
    public bool estUnDefi = true;

    [Header("Paramètres d'Interaction")]
    public KeyCode interactKey = KeyCode.A;
    public Transform joueur;
    public float distanceInteraction = 1.5f;
    public float margeClic = 0.5f;
    public bool startAutomatically = false;   // true : démarre dès que le joueur entre dans la zone
    public bool onlyOnce = false;
    public GameObject hint;                   // Indicateur d'interaction optionnel

    private bool done;
    private bool lancementEnCours;
    static readonly HashSet<string> completedTriggers = new HashSet<string>();

    void Start()
    {
        if (hint != null) hint.SetActive(false);
        if (joueur == null)
        {
            GameObject joueurObjet = GameObject.FindGameObjectWithTag("Player");
            if (joueurObjet != null) joueur = joueurObjet.transform;
        }
        if (joueur == null)
            Debug.LogError("DialogueTrigger : aucun joueur assigné ni trouvé avec le tag Player.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hint != null && !startAutomatically) hint.SetActive(true);
        if (startAutomatically) Launch();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hint != null) hint.SetActive(false);
    }

    void Update()
    {
        bool joueurAssezProche = joueur != null &&
            Vector2.SqrMagnitude(transform.position - joueur.position) <= distanceInteraction * distanceInteraction;
        bool toucheInteraction = false;
#if ENABLE_LEGACY_INPUT_MANAGER
        toucheInteraction = Input.GetKeyDown(interactKey) ||
            Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1);
#endif
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            toucheInteraction |= Keyboard.current.aKey.wasPressedThisFrame ||
                Keyboard.current.digit1Key.wasPressedThisFrame ||
                Keyboard.current.numpad1Key.wasPressedThisFrame;
        }
#endif
        if (joueurAssezProche && toucheInteraction) Launch();

        bool clicGauche = false;
        Vector2 positionEcran = Vector2.zero;
#if ENABLE_LEGACY_INPUT_MANAGER
        clicGauche = Input.GetMouseButtonDown(0);
        positionEcran = Input.mousePosition;
#endif
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            clicGauche |= Mouse.current.leftButton.wasPressedThisFrame;
            positionEcran = Mouse.current.position.ReadValue();
        }
#endif
        if (!clicGauche) return;

        Camera cameraPrincipale = Camera.main;
        Collider2D zoneInteraction = GetComponent<Collider2D>();
        if (cameraPrincipale == null || zoneInteraction == null) return;

        Vector2 positionSouris = cameraPrincipale.ScreenToWorldPoint(positionEcran);
        Vector2 pointProche = zoneInteraction.ClosestPoint(positionSouris);
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Bounds limitesSprite = sprite != null ? sprite.bounds : zoneInteraction.bounds;
        Vector3 pointDansLimites = new Vector3(
            positionSouris.x, positionSouris.y, limitesSprite.center.z);
        if (limitesSprite.Contains(pointDansLimites) ||
            zoneInteraction.OverlapPoint(positionSouris) ||
            Vector2.Distance(positionSouris, pointProche) <= margeClic)
            Launch();
    }

    void Launch()
    {
        if (lancementEnCours || DialogueManager.IsActive) return;
        string triggerKey = CleDeclencheur();
        if (onlyOnce && (done || completedTriggers.Contains(triggerKey))) return;

        if (ouvrirSceneDialogue)
        {
            TextAsset jsonDialogueLancement = jsonDialogue;
            if (jsonDialogueLancement == null && defi == null) defi = FindAnyObjectByType<DefiController>();
            if (jsonDialogueLancement == null && defi != null) jsonDialogueLancement = defi.dialoguesJson;
            if (jsonDialogueLancement == null)
            {
                Debug.LogError("DialogueTrigger : aucun fichier JSON n'est assigné pour ouvrir la scène Dialogue.");
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded(sceneDialogue))
            {
                Debug.LogError("DialogueTrigger : scène de dialogue absente des scènes de build : " + sceneDialogue);
                return;
            }

            DialogueLaunchContext.Preparer(jsonDialogueLancement, fondDialogue, estUnDefi, gameObject.scene.name);
            done = true;
            lancementEnCours = true;
            if (onlyOnce) completedTriggers.Add(triggerKey);
            if (hint != null) hint.SetActive(false);
            if (FadeManager.Instance != null)
                FadeManager.Instance.LoadSceneWithFade(sceneDialogue);
            else
                SceneManager.LoadScene(sceneDialogue);
            return;
        }

        if (defi == null) defi = FindAnyObjectByType<DefiController>();
        if (defi == null)
        {
            Debug.LogError("DialogueTrigger : aucun DefiController trouvé dans la scène.");
            return;
        }

        done = true;
        if (onlyOnce) completedTriggers.Add(triggerKey);
        if (hint != null) hint.SetActive(false);

        TextAsset json = jsonDialogue != null ? jsonDialogue : defi.dialoguesJson;
        defi.Lancer(json, 0);
    }

    string CleDeclencheur()
    {
        string chemin = transform.name;
        Transform parent = transform.parent;
        while (parent != null)
        {
            chemin = parent.name + "/" + chemin;
            parent = parent.parent;
        }
        return gameObject.scene.path + "/" + chemin;
    }
}
