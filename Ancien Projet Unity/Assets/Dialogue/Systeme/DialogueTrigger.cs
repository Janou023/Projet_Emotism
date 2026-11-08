using UnityEngine;

/// <summary>
/// À mettre sur un PNJ / objet avec un Collider2D coché "Is Trigger".
/// Le joueur (tag "Player") s'approche, appuie sur E : le dialogue (JSON) démarre via le DefiController.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Source des Données")]
    public DefiController defi;
    public TextAsset jsonDialogue;            // Glisse 'dialogues_tram.json' ici (sinon celui du DefiController)

    [Header("Paramètres d'Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public bool startAutomatically = false;   // true : démarre dès que le joueur entre dans la zone
    public bool onlyOnce = false;
    public GameObject hint;                   // Petit "E" au-dessus du PNJ (optionnel)

    private bool playerInRange;
    private bool done;

    void Start()
    {
        if (hint != null) hint.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (hint != null && !startAutomatically) hint.SetActive(true);
        if (startAutomatically) Launch();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (hint != null) hint.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey)) Launch();
    }

    void Launch()
    {
        if (DialogueManager.IsActive) return;

        if (defi == null) defi = FindAnyObjectByType<DefiController>();
        if (defi == null)
        {
            Debug.LogError("DialogueTrigger : aucun DefiController trouvé dans la scène.");
            return;
        }
        if (onlyOnce && done) return;

        done = true;
        if (hint != null) hint.SetActive(false);

        TextAsset json = jsonDialogue != null ? jsonDialogue : defi.dialoguesJson;
        defi.Lancer(json, 0);
    }
}
