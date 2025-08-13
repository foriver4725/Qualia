namespace MyScripts.Common;

internal static class RuntimeSettingsInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Screen.SetResolution(1536, 864, true);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }
}