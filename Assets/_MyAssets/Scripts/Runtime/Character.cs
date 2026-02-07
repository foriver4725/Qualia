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
    /// 憑依するアニマのクラス<br/>
    /// 憑依・離脱のトリガーもここで扱う<br/>
    /// サウンドは、一旦SOSのものをそのまま流用する<br/>
    /// </summary>
    internal sealed class Character : MonoBehaviour
    {
        [SerializeField] private CharacterType characterType = CharacterType.Land;
        [SerializeField] private MeshRenderer container;
        [SerializeField] private SpriteRenderer icon;
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
        private static readonly Dictionary<AnimalLeaveInvoker, CancellationTokenSource> s_possessTimerCtsByInvoker = new();
        // 外部公開プロパティ
        #region Public Properties

        internal Transform Transform => transform;
        internal Collider Collider => collider;

        internal CharacterType CharacterType => characterType;
        internal MeshRenderer Container => container;
        internal SpriteRenderer Icon => icon;

        internal Color MaterialColor => container.material.color;

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
                    (this, collider, pc, animalLeaveInvoker, soundPlayer),
                    static (other, param) => (
                        This: param.Item1,
                        SelfCollider: param.collider,
                        PlayerController: param.pc,
                        PossessInvoker: param.animalLeaveInvoker,
                        SoundPlayer: param.soundPlayer,
                        OtherCollider: other
                    )
                )
                .Where(static param => ReferenceEquals(param.OtherCollider, param.PlayerController.Collider))
                .SubscribeAwait(static async (param, ct) =>
                {
                    // 既存タイマーがあれば必ず止める（同じアニマでも時間リセットを保証）
                    if (s_possessTimerCtsByInvoker.TryGetValue(param.PossessInvoker, out var oldCts))
                    {
                        oldCts.Cancel();
                        oldCts.Dispose();
                        s_possessTimerCtsByInvoker.Remove(param.PossessInvoker);
                    }

                    // 上書き獲得：取得済みなら一旦解除してから取り直す（同じ/違う関係なく）
                    if (param.PossessInvoker.IsPossessing)
                    {
                        param.PossessInvoker.LeaveCharacter(param.PlayerController);
                    }

                    // 取得
                    param.PossessInvoker.PossessCharacter(param.This);

                    if (!SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasObtainedAnima)
                    {
                        SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasObtainedAnima = true;
                    }

                    param.PossessInvoker.PossessCharacter_ShowLogIfFirstTime(param.This);

                    LogManager.Instance.ShowManually(string.Empty);
                    LogManager.Instance.ShowAutomatically(
                        ZString.Format("{0} のアニマを取得した", GetName(param.This.characterType))
                    );

                    // 新タイマー開始（Destroy連動 ct とリンク）
                    var newCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    s_possessTimerCtsByInvoker[param.PossessInvoker] = newCts;

                    UniTask.Void(async token =>
                        {
                            param.PossessInvoker.SetDisplayImageFillAmount(1.0f);

                            float duration = param.This.sAnima.PossessDuration;
                            float t = duration;

                            while (t > 0.0f)
                            {
                                await UniTask.NextFrame(cancellationToken: token);

                                t -= Time.deltaTime;
                                param.PossessInvoker.SetDisplayImageFillAmount(t / duration);
                            }

                            param.PossessInvoker.SetDisplayImageFillAmount(0.0f);
                            param.PossessInvoker.LeaveCharacter(param.PlayerController);
                            LogManager2.Instance.ShowAutomatically("アニマの取得状態が解除された");
                        },
                        newCts.Token);
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
