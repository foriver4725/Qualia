namespace MyScripts.Runtime
{
    internal enum CharacterType : byte
    {
        Horse,
    }

    /// <summary>
    /// 憑依する動物のクラス<br/>
    /// 憑依・離脱のトリガーもここで扱う<br/>
    /// サウンドは、一旦SOSのものをそのまま流用する<br/>
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
        [SerializeField] private SOSSoundPlayer soundPlayer;
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
                .Select(
                    (this, collider, pc, animalLeaveInvoker, soundPlayer, TMP_doesLikePlayer),
                    static (other, param) => (
                        This: param.Item1,
                        SelfCollider: param.collider,
                        PlayerController: param.pc,
                        PossessInvoker: param.animalLeaveInvoker,
                        SoundPlayer: param.soundPlayer,
                        OtherCollider: other,
                        TMP_DoesLikePlayer: param.TMP_doesLikePlayer
                    )
                )
                .Where(static param => ReferenceEquals(param.OtherCollider, param.PlayerController.Collider))
                .Where(static param => !param.PossessInvoker.IsPossessing)
                .SubscribeAwait(static async (param, ct) =>
                {
                    if (param.TMP_DoesLikePlayer)
                    {
                        LogManager.Instance.ShowManually("左クリックで憑依");

                        if (await WaitForClickOrExitAsync(param.SelfCollider, param.PlayerController.Collider, ct) == true)
                        {
                            // 憑依する
                            param.PossessInvoker.PossessCharacter(param.PlayerController, param.This);

                            LogManager.Instance.ShowManually(string.Empty);
                            LogManager.Instance.ShowAutomatically("憑依した");

                            param.SoundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);
                        }
                        else
                        {
                            LogManager.Instance.ShowManually(string.Empty);
                        }
                    }
                    else
                    {
                        LogManager.Instance.ShowManually("好感度が足りなくて憑依できない");

                        while (true)
                        {
                            if (await WaitForClickOrExitAsync(param.SelfCollider, param.PlayerController.Collider, ct) == true)
                            {
                                param.SoundPlayer.LetPlay(SSOSSound.Situation.CouldNotRemove);
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
                .Where(static _ => InputManager.InGame.Cancel)
                .Subscribe(static param =>
                {
                    InputManager.InGame.MakeCancelInputDisabledUntilNextFrame();
                    param.Invoker.LeaveCharacter(param.Pc);
                })
                .AddTo(this);
        }

        // Click したなら true を、 Exit したなら false を返す
        // 同フレームなら Exit を優先する
        private static async UniTask<bool> WaitForClickOrExitAsync(Collider selfCollider, Collider playerCollider, Ct ct)
        {
            int i = await UniTask.WhenAny(
                // 同フレームなら Exit を優先するために、このタイミングで待つ
                UniTask.WaitUntil(() => InputManager.InGame.Submit, timing: PlayerLoopTiming.LastUpdate, cancellationToken: ct),
                selfCollider.OnTriggerExitAsObservable()
                    .Where(otherCollider => ReferenceEquals(otherCollider, playerCollider))
                    .FirstAsync(cancellationToken: ct)
                    .AsUniTask()
            );

            if (i == 0)
            {
                InputManager.InGame.MakeSubmitInputDisabledUntilNextFrame();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
