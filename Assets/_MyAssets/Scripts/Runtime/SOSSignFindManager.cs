namespace MyScripts.Runtime
{
    internal sealed class SOSSignFindManager : MonoBehaviour
    {
        [SerializeField] private Collider playerCapsuleCollider;
        [SerializeField] private SSOSSignLogText sosSignLogText;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        internal void Setup(
            ReadOnlyCollection<Collider> sosSignColliders,
            Func<bool> isCharacterHuman,
            Action onFind // スコア更新など、見つけたとき共通の処理
        )
        {
            foreach (Collider sosSignCollider in sosSignColliders)
            {
                Collider col = sosSignCollider;

                col.OnTriggerEnterAsObservable()
                    .Where(c => ReferenceEquals(c, playerCapsuleCollider))
                    .SubscribeAwait(async (c, ct) =>
                    {
                        if (isCharacterHuman?.Invoke() == true)
                        {
                            LogManager.Instance.ShowManually("左クリックで取り除く");

                            if (await WaitForClickOrExit(col, ct) == true)
                            {
                                // 取り除く
                                col.gameObject.SetActive(false);
                                onFind?.Invoke();

                                LogManager.Instance.ShowManually(string.Empty);
                                LogManager.Instance.ShowAutomatically(
                                    sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnHumanClick)
                                );

                                soundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);
                            }
                            else
                            {
                                LogManager.Instance.ShowManually(string.Empty);
                            }
                        }
                        else
                        {
                            {
                                using var sb = ZString.CreateStringBuilder();
                                sb.AppendFormat("{0}\n(人間でないと取り除けない)", sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnAnimalApproach));
                                LogManager.Instance.ShowManually(sb);
                            }

                            while (true)
                            {
                                if (await WaitForClickOrExit(col, ct) == true)
                                {
                                    soundPlayer.LetPlay(SSOSSound.Situation.CouldNotRemove);
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            LogManager.Instance.ShowManually(string.Empty);
                        }
                    })
                    .AddTo(col);
            }
        }

        // Click したなら true を、 Exit したなら false を返す
        // 同フレームなら Exit を優先する
        private async UniTask<bool> WaitForClickOrExit(Collider collider, Ct ct) => await UniTask.WhenAny(
            // 同フレームなら Exit を優先するために、このタイミングで待つ
            UniTask.WaitUntil(() => InputManager.InGameSubmit.Bool, timing: PlayerLoopTiming.LastUpdate, cancellationToken: ct),
            collider.OnTriggerExitAsObservable()
                .Where(c => ReferenceEquals(c, playerCapsuleCollider))
                .FirstAsync(cancellationToken: ct)
                .AsUniTask()
        ) == 0;
    }
}
