namespace MyScripts.Common;

internal static class UniTaskExtension
{
    internal static async UniTask SecAwait(
        this float sec,
        bool ignoreTimeScale = false,
        PlayerLoopTiming timing = PlayerLoopTiming.Update,
        Ct ct = default,
        bool cancelImmediately = false
    ) => await UniTask.WaitForSeconds(sec, ignoreTimeScale, timing, ct, cancelImmediately);

    internal static async UniTask SecAwaitThenDo(
        this float sec,
        Action act,
        bool ignoreTimeScale = false,
        PlayerLoopTiming timing = PlayerLoopTiming.Update,
        Ct ct = default,
        bool cancelImmediately = false
    )
    {
        await sec.SecAwait(ignoreTimeScale, timing, ct, cancelImmediately);
        act?.Invoke();
    }

    internal static async UniTask SecAwaitThenAwait(
        this float sec,
        Func<Ct, UniTask> task,
        bool ignoreTimeScale = false,
        PlayerLoopTiming timing = PlayerLoopTiming.Update,
        Ct ct = default,
        bool cancelImmediately = false
    )
    {
        await sec.SecAwait(ignoreTimeScale, timing, ct, cancelImmediately);
        if (task != null)
            await task(ct);
    }
}
