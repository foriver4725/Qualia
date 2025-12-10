using MyScripts.Common.SaveSystem;
using MyScripts.Runtime.Log;

namespace MyScripts.Runtime
{
    internal enum CharacterType : byte
    {
        None, // 憑依していない状態を表すことが出来る
        Horse,
        Shellfish,
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
        [SerializeField, Range(0.0f, 100.0f), Tooltip("SOSサインの残り度(%)がこれ以下になったら、憑依できるようになる")]
        private float possessableLimitSOSSignLeftRatio = 50.0f;
        [Space(10)]
        [SerializeField] private new Transform transform;
        [SerializeField] private new Collider collider;
        [Space(10)]
        [SerializeField] private Renderer[] horseRenderers;
        [SerializeField] private Renderer[] shellfishRenderers;
        [Space(10)]
        [SerializeField] private PlayerController pc;
        [SerializeField] private AnimalLeaveInvoker animalLeaveInvoker;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        // 外部公開プロパティ
        #region Public Properties

        internal Transform Transform => transform;
        internal Collider Collider => collider;

        internal CharacterType CharacterType => characterType;
        internal Behaviour NameText => nameText;

        internal void UpdateModel(CharacterType type)
        {
            foreach (var renderer in horseRenderers.AsSpan())
                renderer.enabled = (type == CharacterType.Horse);
            foreach (var renderer in shellfishRenderers.AsSpan())
                renderer.enabled = (type == CharacterType.Shellfish);
        }

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
                CharacterType.Shellfish => "貝",
                _ => throw new ArgumentOutOfRangeException(nameof(characterType), characterType, null)
            };

            UpdateModel(characterType);

            // 憑依
            collider.OnTriggerEnterAsObservable()
                .Select(
                    (this, collider, pc, animalLeaveInvoker, soundPlayer, possessableLimitSOSSignLeftRatio),
                    static (other, param) => (
                        This: param.Item1,
                        SelfCollider: param.collider,
                        PlayerController: param.pc,
                        PossessInvoker: param.animalLeaveInvoker,
                        SoundPlayer: param.soundPlayer,
                        OtherCollider: other,
                        PossessableLimit: param.possessableLimitSOSSignLeftRatio
                    )
                )
                .Where(static param => ReferenceEquals(param.OtherCollider, param.PlayerController.Collider))
                .SubscribeAwait(static async (param, ct) =>
                {
                    if (!param.PossessInvoker.IsPossessing)
                    {
                        // SOSサインの残り度が一定以下なら、憑依可能
                        if (CalculateCurrentSOSSignLeftRatio() * 100.0f <= param.PossessableLimit)
                        {
                            LogManager.Instance.ShowManually("左クリックで憑依");

                            if (await WaitForClickOrExitAsync(param.SelfCollider, param.PlayerController.Collider, ct) == true)
                            {
                                // 憑依する
                                param.PossessInvoker.PossessCharacter(param.PlayerController, param.This);

                                LogManager.Instance.ShowManually(string.Empty);
                                LogManager.Instance.ShowAutomatically("憑依した");

                                // これは AnimalLeaveInvoker 側で再生する (離脱時のサウンド再生も、一緒のクラスで行いたいので)
                                // .// TODO: SOSサインのサウンドを使いまわす!
                                // param.SoundPlayer.LetPlay(SSOSSound.Situation.CouldRemove);
                            }
                            else
                            {
                                LogManager.Instance.ShowManually(string.Empty);
                            }
                        }
                        else
                        {
                            LogManager.Instance.ShowManually(
                                ZString.Format("穢れ度が {0:F2}% 以下じゃないと\n憑依できないよ!", param.PossessableLimit));

                            while (true)
                            {
                                if (await WaitForClickOrExitAsync(param.SelfCollider, param.PlayerController.Collider, ct) == true)
                                {
                                    // TODO: SOSサインのサウンドを使いまわす!
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
                    }
                    else
                    {
                        LogManager.Instance.ShowManually("憑依中は再度憑依できません");

                        while (true)
                        {
                            if (await WaitForClickOrExitAsync(param.SelfCollider, param.PlayerController.Collider, ct) == true)
                            {
                                // TODO: SOSサインのサウンドを使いまわす!
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

        // SOSサインが残っている割合を計算する [0, 1]
        // 他クラスで同様の処理を行ったりはしているが、他 MonoBehaviour への依存は極力避けたいので、
        // 逐一ここで計算するものとする
        private static float CalculateCurrentSOSSignLeftRatio()
        {
            Span<bool> foundSOSSigns = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasFoundSOSSigns.AsSpan();
            int leftCount = 0;
            foreach (bool hasFound in foundSOSSigns)
            {
                if (!hasFound)
                {
                    leftCount++;
                }
            }
            return 1.0f * leftCount / foundSOSSigns.Length;
        }
    }
}
