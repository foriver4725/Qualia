namespace MyScripts.Common;

internal static class Initializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void Init()
    {
        Screen.SetResolution(1920, 1080, true);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        ConnectSteamAsync().Forget();
    }

    private static async UniTaskVoid ConnectSteamAsync()
    {
        // 初期化が成功するまで、リトライし続ける
        while (!Steam.APIConnector.Init())
        {
            await UniTask.DelayFrame(16);
        }
    }
}
