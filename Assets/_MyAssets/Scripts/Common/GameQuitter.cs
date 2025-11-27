namespace MyScripts.Common;

internal static class GameQuitter
{
    internal static void InvokeQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
