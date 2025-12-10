using MyScripts.Common.SaveSystem;
using MyScripts.Runtime.Log;

namespace MyScripts.Runtime
{
    internal enum CharacterType : byte
    {
        None, // 憑依していない状態を表すことが出来る
        Land,
        Sea,
        Sky,
    }

    /// <summary>
    /// 憑依する動物のクラス<br/>
    /// 憑依・離脱のトリガーもここで扱う<br/>
    /// サウンドは、一旦SOSのものをそのまま流用する<br/>
    /// </summary>
    internal sealed class Character : MonoBehaviour
    {
        [SerializeField] private CharacterType characterType = CharacterType.Land;
        [SerializeField] private MeshRenderer container;
        [SerializeField] private SpriteRenderer icon;
        [SerializeField, Range(0.0f, 100.0f), Tooltip("SOSサインの残り度(%)がこれ以下になったら、憑依できるようになる")]
        private float possessableLimitSOSSignLeftRatio = 50.0f;
        [Space(10)]
        [SerializeField] private new Transform transform;
        [SerializeField] private new Collider collider;
        [Space(10)]
        [SerializeField] private SAnima sAnima;
        [SerializeField] private Material landMaterial;
        [SerializeField] private Material seaMaterial;
        [SerializeField] private Material skyMaterial;
        [Space(10)]
        [SerializeField] private PlayerController pc;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AnimalLeaveInvoker animalLeaveInvoker;
        [SerializeField] private SOSSoundPlayer soundPlayer;

        // 外部公開プロパティ
        #region Public Properties

        internal Transform Transform => transform;
        internal Collider Collider => collider;

        internal CharacterType CharacterType => characterType;
        internal MeshRenderer Container => container;
        internal SpriteRenderer Icon => icon;

        internal bool SetVisible(bool isVisible)
        {
            container.enabled = isVisible;
            icon.enabled = isVisible;
            return isVisible;
        }

        internal void Teleport(Vector3 position, Vector3 forward)
        {
            transform.position = position;
            transform.forward = forward;
        }

        #endregion

        private void Awake()
        {
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
                        LogManager.Instance.ShowManually("左クリックで取得");

                        if (await WaitForClickOrExitAsync(param.SelfCollider, param.PlayerController.Collider, ct) == true)
                        {
                            // 取得する
                            param.PossessInvoker.PossessCharacter(param.This);

                            LogManager.Instance.ShowManually(string.Empty);
                            LogManager.Instance.ShowAutomatically(ZString.Format("{0} のアニマを取得した", GetName(param.This.characterType)));

                            // 一定時間経過後に、解除するようにする
                            UniTask.Void(async ct =>
                            {
                                await param.This.sAnima.PossessDuration.SecAwait(ct: ct);

                                if (param.PossessInvoker.IsPossessing)
                                    param.PossessInvoker.LeaveCharacter(param.PlayerController);
                            },
                            cancellationToken: ct);

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
        }

        private void LateUpdate()
        {
            // プレイヤーの方を向く
            if (icon)
            {
                Vector3 directionToCamera = playerCamera.transform.position - icon.transform.position;
                directionToCamera.y = 0;
                if (directionToCamera != Vector3.zero)
                    icon.transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }

        private void UpdateModel(CharacterType type)
        {
            if (type == CharacterType.None)
            {
                // 見えなくする
                container.enabled = false;
                icon.enabled = false;
                return;
            }

            // 見えるようにする
            container.enabled = true;
            icon.enabled = true;

            container.material = type switch
            {
                CharacterType.Land => landMaterial,
                CharacterType.Sea => seaMaterial,
                CharacterType.Sky => skyMaterial,
                _ => null
            };

            icon.sprite = type switch
            {
                CharacterType.Land => sAnima.LandIcon,
                CharacterType.Sea => sAnima.SeaIcon,
                CharacterType.Sky => sAnima.SkyIcon,
                _ => null
            };
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

        private static string GetName(CharacterType type) => type switch
        {
            CharacterType.Land => "陸",
            CharacterType.Sea => "海",
            CharacterType.Sky => "空",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
