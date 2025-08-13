namespace MyScripts.Common;

internal static class RuntimeSettingsInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Run()
    {
        Screen.SetResolution(1920, 1080, true);
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 30;
    }
}