namespace MyScripts.Common;

internal static class RuntimeSettingsInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Screen.SetResolution(1920, 1080, true);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }
}