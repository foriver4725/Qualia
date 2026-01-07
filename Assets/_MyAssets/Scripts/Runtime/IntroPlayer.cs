using MyScripts.Runtime.UI.Main;

namespace MyScripts.Runtime
{
    /// <summary>
    /// 初めてゲームを開始した際に、最初に流れるイントロ映像を再生するクラス
    /// </summary>
    internal sealed class IntroPlayer : MonoBehaviour
    {
        [SerializeField, Range(0.0f, 10.0f)] private float durationUntilPlay = 0.5f;
        [SerializeField] private PauseInvoker pauseInvoker;

        // 他の初期化後に実行する
        private void Start() => ImplAsync(destroyCancellationToken).Forget();

        private async UniTaskVoid ImplAsync(Ct ct)
        {
            if (!PlayInfo.IsFirstPlay)
                return;

            await durationUntilPlay.SecAwait(ct: ct);
            // ポーズでなくなるまで待機
            await UniTask.WaitUntil(() => pauseInvoker.IsPaused == false, cancellationToken: ct);

            CutScenePlayer.Instance.Play(SCutScene.CutSceneType.Intro);
        }
    }
}
