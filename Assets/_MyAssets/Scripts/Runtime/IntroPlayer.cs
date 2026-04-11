using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime
{
    /// <summary>
    /// 初めてゲームを開始した際に、最初に流れるイントロ画像シーケンスを再生するクラス
    /// </summary>
    internal sealed class IntroPlayer : MonoBehaviour
    {
        [SerializeField] private Sprite[] sequenceSprites;
        [SerializeField, Range(0.0f, 10.0f)] private float durationUntilPlay = 0.5f;
        [SerializeField] private ImageSequencePlayerPlayOptions playOptions;
        [SerializeField] private PauseInvoker pauseInvoker;

        // 他の初期化後に実行する
        private void Start() => ImplAsync(destroyCancellationToken).Forget();

        private async UniTaskVoid ImplAsync(Ct ct)
        {
            if (!Variables.IsFirstPlay)
                return;

            await durationUntilPlay.SecAwait(ct: ct);
            // ポーズでなくなるまで待機
            await UniTask.WaitUntil(pauseInvoker,
                static pauseInvoker => !pauseInvoker.IsPaused, cancellationToken: ct);

            // まず動画を再生し、終了後にチュートリアルシーケンスを再生
            await CutScenePlayer.Instance.PlayAsync(ct);
            await ImageSequencePlayer.Instance.PlayAsync(sequenceSprites, playOptions, ct);
        }
    }
}