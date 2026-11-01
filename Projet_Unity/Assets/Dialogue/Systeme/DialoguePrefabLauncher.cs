using UnityEngine;

public class DialoguePrefabLauncher : MonoBehaviour
{
    public DialogueUIBootstrap dialoguePrefab;
    public bool demarrerAuLancement = true;

    DialogueUIBootstrap instance;

    void Awake()
    {
        if (dialoguePrefab == null)
        {
            Debug.LogError("DialoguePrefabLauncher : aucun prefab de dialogue n'est assigné.");
            return;
        }

        instance = Instantiate(dialoguePrefab);
    }

    void Start()
    {
        if (demarrerAuLancement && instance != null)
            instance.LancerDialogue();
    }
}
