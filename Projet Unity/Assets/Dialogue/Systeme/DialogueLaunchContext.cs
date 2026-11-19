using UnityEngine;

public struct DialogueLaunchSettings
{
    public TextAsset Json;
    public Sprite Fond;
    public bool EstDefi;
    public string SceneRetour;
    public bool EcranReussiteSimple;
    public string IdDefi;
}

public static class DialogueLaunchContext
{
    static DialogueLaunchSettings settings;
    static bool isPending;

    public static void Preparer(TextAsset json, Sprite fond, bool estDefi, string sceneRetour,
        bool ecranReussiteSimple = false, string idDefi = "")
    {
        settings = new DialogueLaunchSettings
        {
            Json = json,
            Fond = fond,
            EstDefi = estDefi,
            SceneRetour = sceneRetour,
            EcranReussiteSimple = ecranReussiteSimple,
            IdDefi = idDefi
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
