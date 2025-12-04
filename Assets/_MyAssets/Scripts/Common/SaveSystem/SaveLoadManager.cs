namespace MyScripts.Common.SaveSystem;

internal static class SaveLoadManager
{
    private static Data data;
    internal static Data Data => data;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void OnGameBegin()
    {
        SaveLoadInvoker.Load(out data);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += OnGamePlayModeStateChanged;

        static void OnGamePlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                OnGameEnd();
            }
        }
#else
        Application.quitting += OnGameEnd;
#endif
    }

    private static void OnGameEnd()
    {
        SaveLoadInvoker.Save(data);
    }
}
