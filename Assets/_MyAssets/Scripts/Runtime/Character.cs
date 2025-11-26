namespace MyScripts.Runtime
{
    internal sealed class Character : MonoBehaviour
    {
        private enum CharacterType : byte
        {
            Horse,
        }

        [SerializeField] private CharacterType characterType = CharacterType.Horse;
        [SerializeField] private TextMeshPro nameText;
        [SerializeField] private Collider myCollider;
        [SerializeField] private Collider playerCollider;
        [SerializeField] private bool TMP_doesLikePlayer = true; // テスト用

        private void Awake()
        {
            nameText.text = characterType switch
            {
                CharacterType.Horse => "馬",
                _ => throw new ArgumentOutOfRangeException(nameof(characterType), characterType, null)
            };

            myCollider.enabled = true;
            myCollider.OnTriggerEnterAsObservable()
                .Where(c => ReferenceEquals(c, playerCollider))
                .SubscribeAwait(myCollider, async (c, col, ct) =>
            {
                if (TMP_doesLikePlayer)
                {
                    LogManager.Instance.ShowManually("左クリックで憑依");

                    if (await WaitForClickOrExitAsync(col, ct) == true)
                    {
                        // 憑依する
                        myCollider.enabled = false;
                        {
                            "憑依した".Print();
                        }
                    }

                    LogManager.Instance.ShowManually(string.Empty);
                }
                else
                {
                    LogManager.Instance.ShowManually("好感度が足りなくて憑依できない");

                    while (true)
                    {
                        if (await WaitForClickOrExitAsync(col, ct) == true)
                        {
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
                .AddTo(myCollider);
        }

        // Click したなら true を、 Exit したなら false を返す
        // 同フレームなら Exit を優先する
        private async UniTask<bool> WaitForClickOrExitAsync(Collider collider, Ct ct) => await UniTask.WhenAny(
            // 同フレームなら Exit を優先するために、このタイミングで待つ
            UniTask.WaitUntil(() => InputManager.InGameSubmit.Bool, timing: PlayerLoopTiming.LastUpdate, cancellationToken: ct),
            collider.OnTriggerExitAsObservable()
                .Where(c => ReferenceEquals(c, playerCollider))
                .FirstAsync(cancellationToken: ct)
                .AsUniTask()
        ) == 0;
    }
}
