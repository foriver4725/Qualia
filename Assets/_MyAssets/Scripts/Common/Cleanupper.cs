namespace MyScripts.Common;

internal static class Cleanupper
{
    /// <summary>
    /// リソースのアンロードを非同期で行い、その後ガベージコレクションを同期で実行する
    /// </summary>
    internal static async UniTask RunAsync(Ct ct = default)
    {
        await Resources.UnloadUnusedAssets().WithCancellation(ct);
        await UniTask.NextFrame(cancellationToken: ct);
        GC.Collect();
    }

    /// <summary>
    /// ガベージコレクションのみを同期で実行する
    /// </summary>
    internal static void RunOnlyGC()
    {
        GC.Collect();
    }
}