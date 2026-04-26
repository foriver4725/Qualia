using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime
{
    /// <summary>
    /// 初めてゲームを開始した際に、最初に流れるイントロを再生するクラス
    /// </summary>
    internal sealed class IntroPlayer : MonoBehaviour
    {
        [SerializeField] private Sprite[] sequenceSprites;
        [SerializeField, Range(0.0f, 10.0f)] private float durationUntilStoryMovie = 0.5f;
        [SerializeField, Range(0.0f, 10.0f)] private float durationUntilTutorialImages = 1.0f;
        [SerializeField] private ImageSequencePlayerPlayOptions playOptions;
        [SerializeField] private PauseInvoker pauseInvoker;
        [SerializeField] private SStoryMovie storyMovie;

        // 他の初期化後に実行する
        private void Start() => ImplAsync(destroyCancellationToken).Forget();

        private async UniTaskVoid ImplAsync(Ct ct)
        {
            if (!Variables.IsFirstPlay)
                return;

            // await durationUntilStoryMovie.SecAwait(ct: ct);
            // await WaitUntilUnPaused(ct);
            // await CutScenePlayer.Instance.PlayAsync(storyMovie.Get(SStoryMovie.GameProgress.P0), ct);

            await durationUntilTutorialImages.SecAwait(ct: ct);
            await WaitUntilUnPaused(ct);
            await ImageSequencePlayer.Instance.PlayAsync(sequenceSprites, playOptions, ct);
        }

        // ポーズでなくなるまで待機する
        private async UniTask WaitUntilUnPaused(Ct ct)
        {
            await UniTask.WaitUntil(pauseInvoker,
                static pauseInvoker => !pauseInvoker.IsPaused, cancellationToken: ct);
        }
    }
}