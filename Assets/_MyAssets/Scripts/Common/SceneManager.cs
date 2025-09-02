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
    /// <param name="beforeCleanupBegin">クリーンアップ開始直前のコールバック</param>
    /// <param name="onCleanupAsync">クリーンアップ中,並列で実行されるタスク (完了時にキャンセルされる)</param>
    /// <param name="afterCleanupEnd">クリーンアップ終了直後のコールバック</param>
    /// <param name="beforeLoadBegin">ロード開始直前のコールバック</param>
    /// <param name="onLoad">ロード中,並列で実行されるタスク (完了時にキャンセルされる)</param>
    /// <param name="onLoadProgressChanged">ロード進捗のコールバック</param>
    /// <param name="afterLoadEnd">ロード終了直後のコールバック</param>
    /// <param name="afterLoadEndBeforeSceneTriggerInvokeBegin">ロード終了後にシーン遷移をトリガーするタスクの,開始直前のコールバック</param>
    /// <param name="afterLoadEndUntilSceneTriggerInvokeAsync">ロード終了後にシーン遷移をトリガーするタスク (このタスクが完了したらシーン遷移処理が実行される. キャンセル不可)</param>
    /// <param name="afterLoadEndAfterSceneTriggerInvokeEnd">ロード終了後にシーン遷移をトリガーするタスクの,終了直後のコールバック (この後、直ちにシーン遷移処理が実行される)</param>
    internal static async UniTaskVoid LoadAsync(
        this Scene scene,

        Action beforeCleanupBegin = null,
        Func<Ct, UniTaskVoid> onCleanupAsync = null,
        Action afterCleanupEnd = null,

        Action beforeLoadBegin = null,
        Func<Ct, UniTaskVoid> onLoad = null,
        Action<float> onLoadProgressChanged = null,
        Action afterLoadEnd = null,

        Action afterLoadEndBeforeSceneTriggerInvokeBegin = null,
        Func<UniTask> afterLoadEndUntilSceneTriggerInvokeAsync = null,
        Action afterLoadEndAfterSceneTriggerInvokeEnd = null
    )
    {
        // クリーンアップ
        // シーン遷移中は Resources.UnloadUnusedAssets() が完了しないため、ここで実行する
        {
            beforeCleanupBegin?.Invoke();

            bool doParallelAsync = onCleanupAsync != null;
            Cts cts = null;

            if (doParallelAsync)
            {
                cts = new();
                onCleanupAsync(cts.Token).Forget();
            }

            {
                await Cleanupper.RunAsync(Ct.None);
            }

            if (doParallelAsync)
            {
                cts.Cancel();
                cts.Dispose();
            }

            afterCleanupEnd?.Invoke();

            await UniTask.NextFrame();
        }

        // ロード
        {
            beforeLoadBegin?.Invoke();

            bool doParallelAsync = onLoad != null;
            Cts cts = null;

            if (doParallelAsync)
            {
                cts = new();
                onLoad(cts.Token).Forget();
            }

            // 最後のシーン遷移トリガーでも使うので、ここのスコープで変数に保持している
            AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneNames[scene]);
            op.allowSceneActivation = false;
            {
                const float ProgressReportThreshold = 0.01f; // これ以上更新されたら、コールバックを発火
                float progressReported = -1;
                onLoadProgressChanged?.Invoke(0.0f);

                while (true)
                {
                    float progressRaw = op.progress;
                    float progress = Mathf.Clamp01(progressRaw / 0.9f);

                    if (Mathf.Abs(progress - progressReported) >= ProgressReportThreshold)
                    {
                        progressReported = progress;
                        onLoadProgressChanged?.Invoke(progress);
                    }

                    if (progressRaw >= 0.9f)
                    {
                        onLoadProgressChanged?.Invoke(1.0f);
                        break;
                    }

                    await UniTask.NextFrame();
                }
            }

            if (doParallelAsync)
            {
                cts.Cancel();
                cts.Dispose();
            }

            afterLoadEnd?.Invoke();

            await UniTask.NextFrame();

            // 完了
            {
                afterLoadEndBeforeSceneTriggerInvokeBegin?.Invoke();

                if (afterLoadEndUntilSceneTriggerInvokeAsync != null)
                    await afterLoadEndUntilSceneTriggerInvokeAsync();

                afterLoadEndAfterSceneTriggerInvokeEnd?.Invoke();

                await UniTask.NextFrame();

                op.allowSceneActivation = true;
            }
        }
    }
}
