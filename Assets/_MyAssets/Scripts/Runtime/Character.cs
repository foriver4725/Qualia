namespace MyScripts.Runtime
{
    internal enum CharacterType : byte
    {
        Horse,
    }

    /// <summary>
    /// 憑依する動物のクラス
    /// 憑依・離脱のトリガーもここで扱う
    /// </summary>
    internal sealed class Character : MonoBehaviour
    {
        [SerializeField] private CharacterType characterType = CharacterType.Horse;
        [SerializeField] private TextMeshPro nameText;
        [Space(10)]
        [SerializeField] private new Transform transform;
        [SerializeField] private new Renderer renderer;
        [SerializeField] private new Collider collider;
        [Space(10)]
        [SerializeField] private PlayerController pc;
        [SerializeField] private AnimalLeaveInvoker animalLeaveInvoker;
        [SerializeField] private bool TMP_doesLikePlayer = true; // テスト用

        // 外部公開プロパティ
        #region Public Properties

        internal Transform Transform => transform;
        internal Renderer Renderer => renderer;
        internal Collider Collider => collider;

        internal CharacterType CharacterType => characterType;
        internal Behaviour NameText => nameText;

        internal void Teleport(Vector3 position, Vector3 forward)
        {
            transform.position = position;
            transform.forward = forward;
        }

        #endregion

        private void Awake()
        {
            nameText.text = characterType switch
            {
                CharacterType.Horse => "馬",
                _ => throw new ArgumentOutOfRangeException(nameof(characterType), characterType, null)
            };

            // 憑依
            collider.OnTriggerEnterAsObservable()
                .Where(c => ReferenceEquals(c, pc.Collider))
                .SubscribeAwait(collider, async (c, col, ct) =>
            {
                if (TMP_doesLikePlayer)
                {
                    LogManager.Instance.ShowManually("左クリックで憑依");

                    if (await WaitForClickOrExitAsync(col, ct) == true)
                        animalLeaveInvoker.PossessCharacter(pc, this);

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
                .AddTo(collider);

            // 離脱
            this.UpdateAsObservable()
                .Select((animalLeaveInvoker, pc), static (_, param) => (Invoker: param.animalLeaveInvoker, Pc: param.pc))
                .Where(static param => param.Invoker.IsPossessing)
                .Where(static _ => InputManager.InGameLeaveAnimal.Bool)
                .Subscribe(static param =>
                {
                    param.Invoker.LeaveCharacter(param.Pc);
                })
                .AddTo(this);
        }

        // Click したなら true を、 Exit したなら false を返す
        // 同フレームなら Exit を優先する
        private async UniTask<bool> WaitForClickOrExitAsync(Collider collider, Ct ct) => await UniTask.WhenAny(
            // 同フレームなら Exit を優先するために、このタイミングで待つ
            UniTask.WaitUntil(() => InputManager.InGameSubmit.Bool, timing: PlayerLoopTiming.LastUpdate, cancellationToken: ct),
            collider.OnTriggerExitAsObservable()
                .Where(c => ReferenceEquals(c, pc.Collider))
                .FirstAsync(cancellationToken: ct)
                .AsUniTask()
        ) == 0;
    }
}
