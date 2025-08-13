namespace MyScripts.Common;

internal enum Scene : byte
{
    Title,
    Main,
    Result,
}

internal static class SceneManager
{
    private static readonly Dictionary<Scene, string> sceneNames = new()
    {
        { Scene.Title, "Title" },
        { Scene.Main, "Main" },
        { Scene.Result, "Result" }
    };

    /// <summary>
    /// シーンの非同期ロード (キャンセル不可)
    /// </summary>
    /// <param name="scene">遷移するシーン</param>
    /// <param name="onDoingCleanupAsync">リソースを解放中の演出用タスク (メソッド内でキャンセルされる)</param>
    /// <param name="onProgressChanged">ロード進捗が更新された時のコールバック</param>
    /// <param name="onCompletedAsync">すべて完了後の演出用タスク (キャンセル不可)</param>
    /// <returns></returns>
    internal static async UniTaskVoid LoadAsync(
        this Scene scene,
        Func<Ct, UniTaskVoid> onDoingCleanupAsync = null,
        Action<float> onProgressChanged = null,
        Func<UniTask> onCompletedAsync = null
    )
    {
        // クリーンアップ
        // シーン遷移中は Resources.UnloadUnusedAssets() が完了しないため、ここで実行する
        if (onDoingCleanupAsync != null)
        {
            Cts ctsOnDoingCleanup = new();
            onDoingCleanupAsync(ctsOnDoingCleanup.Token).Forget();

            await Cleanupper.RunAsync(Ct.None);

            ctsOnDoingCleanup.Cancel();
            ctsOnDoingCleanup.Dispose();
        }
        else
        {
            await Cleanupper.RunAsync(Ct.None);
        }
        await UniTask.NextFrame();

        // ロード準備
        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneNames[scene]);
        op.allowSceneActivation = false;

        // ロード
        const float ProgressReportThreshold = 0.01f; // これ以上更新されたら、コールバックを発火
        float progressReported = -1;
        onProgressChanged?.Invoke(0.0f);
        while (true)
        {
            float progressRaw = op.progress;
            float progress = Mathf.Clamp01(progressRaw / 0.9f);

            if (Mathf.Abs(progress - progressReported) >= ProgressReportThreshold)
            {
                progressReported = progress;
                onProgressChanged?.Invoke(progress);
            }

            if (progressRaw >= 0.9f)
                break;

            await UniTask.NextFrame();
        }

        // 完了
        if (onCompletedAsync != null)
            await onCompletedAsync();
        await UniTask.NextFrame();

        op.allowSceneActivation = true;
    }
}
