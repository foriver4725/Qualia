using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime
{
    internal sealed class DemoClearDetector : MonoBehaviour
    {
        [SerializeField] private Sprite[] sequenceSprites;
        [SerializeField, Range(0.0f, 10.0f)] private float durationUntilClearImages = 0.5f;
        [SerializeField] private ImageSequencePlayerPlayOptions playOptions;
        [SerializeField] private PauseInvoker pauseInvoker;
        [SerializeField] private SSOSAnimaArrangement sosAnimaArrangement;

        private void Start() => ImplAsync(destroyCancellationToken).Forget();

        private async UniTaskVoid ImplAsync(Ct ct)
        {
            ct.ThrowIfCancellationRequested();

            await UniTask.WaitUntil(sosAnimaArrangement, static sosAnimaArrangement =>
            {
                ReadOnlySpan<bool> foundSOSSigns
                    = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasFoundSOSSigns.AsSpan();

                int foundCount = 0;
                foreach (bool foundSOSSign in foundSOSSigns)
                {
                    if (foundSOSSign)
                        foundCount++;
                }

                return foundCount >= sosAnimaArrangement.DemoClearSOSCount;
            }, cancellationToken: ct);

            await durationUntilClearImages.SecAwait(ct: ct);
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