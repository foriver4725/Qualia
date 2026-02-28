namespace MyScripts.Common;

internal static class Initializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void Init()
    {
        ConnectSteamAsync().Forget();
    }

    private static async UniTaskVoid ConnectSteamAsync()
    {
        // 初期化が成功するまで、リトライし続ける
        while (!Steam.APIConnector.Init())
        {
            await UniTask.DelayFrame(16);
        }

        "Steam API Connected".Print();
    }
}