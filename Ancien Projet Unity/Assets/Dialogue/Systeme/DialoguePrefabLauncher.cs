using UnityEngine;

public class DialoguePrefabLauncher : MonoBehaviour
{
    public DialogueUIBootstrap dialoguePrefab;
    public bool demarrerAuLancement = true;

    DialogueUIBootstrap instance;
    DialogueLaunchSettings launchSettings;
    bool hasLaunchSettings;

    void Awake()
    {
        hasLaunchSettings = DialogueLaunchContext.Consommer(out launchSettings);
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
        {
            if (hasLaunchSettings)
                instance.LancerDialogue(launchSettings.Json, launchSettings.Fond,
                    launchSettings.EstDefi, launchSettings.SceneRetour);
            else
                instance.LancerDialogue();
        }
    }
}
