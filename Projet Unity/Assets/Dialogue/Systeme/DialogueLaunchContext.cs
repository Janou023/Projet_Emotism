using UnityEngine;

public struct DialogueLaunchSettings
{
    public TextAsset Json;
    public Sprite Fond;
    public bool EstDefi;
    public string SceneRetour;
}

public static class DialogueLaunchContext
{
    static DialogueLaunchSettings settings;
    static bool isPending;

    public static void Preparer(TextAsset json, Sprite fond, bool estDefi, string sceneRetour)
    {
        settings = new DialogueLaunchSettings
        {
            Json = json,
            Fond = fond,
            EstDefi = estDefi,
            SceneRetour = sceneRetour
        };
        isPending = true;
    }

    public static bool Consommer(out DialogueLaunchSettings launchSettings)
    {
        launchSettings = settings;
        bool result = isPending;
        settings = default(DialogueLaunchSettings);
        isPending = false;
        return result;
    }
}
