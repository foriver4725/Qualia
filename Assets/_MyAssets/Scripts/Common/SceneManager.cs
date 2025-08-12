namespace MyScripts.Common;

internal enum Scene : byte
{
    Main,
    Result,
}

internal static class SceneManager
{
    private static readonly Dictionary<Scene, string> sceneNames = new()
    {
        { Scene.Main, "Main" },
        { Scene.Result, "Result" }
    };

    internal static void Load(this Scene scene)
        => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneNames[scene]);
}
