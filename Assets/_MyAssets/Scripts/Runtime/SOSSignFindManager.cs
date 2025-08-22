using MyScripts.SO.Parameter;

namespace MyScripts.Runtime
{
    internal sealed class SOSSignFindManager : MonoBehaviour
    {
        [SerializeField] private Collider playerCapsuleCollider;
        [SerializeField] private SSOSSignLogText sosSignLogText;

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

                            // 決定の入力 or TriggerExit まで待つ
                            int i = await UniTask.WhenAny(
                                UniTask.WaitUntil(() => InputManager.InGameSubmit.Bool, cancellationToken: ct),
                                col.OnTriggerExitAsObservable()
                                    .Where(c => ReferenceEquals(c, playerCapsuleCollider))
                                    .FirstAsync(cancellationToken: ct)
                                    .AsUniTask()
                            );

                            if (i == 0) // 決定された
                            {
                                // 取り除く
                                col.gameObject.SetActive(false);
                                onFind?.Invoke();

                                LogManager.Instance.ShowManually(string.Empty);
                                LogManager.Instance.ShowAutomatically(
                                    sosSignLogText.GetRandom(SSOSSignLogText.LogType.OnHumanClick)
                                );
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

                            // TriggerExit まで待つ
                            await col.OnTriggerExitAsObservable()
                                .Where(c => ReferenceEquals(c, playerCapsuleCollider))
                                .FirstAsync(cancellationToken: ct)
                                .AsUniTask();

                            LogManager.Instance.ShowManually(string.Empty);
                        }
                    })
                    .AddTo(col);
            }
        }
    }
}