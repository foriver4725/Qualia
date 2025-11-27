namespace MyScripts.Common;

internal static class GameQuitter
{
    internal static bool HasInvoked { get; private set; } = false;

    internal static void InvokeQuit()
    {
        if (HasInvoked) return;
        HasInvoked |= true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
