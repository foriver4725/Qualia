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
                    // 接触したら即取得。取得済みでも上書きして獲得（効果時間もリセット）
                    if (param.PossessInvoker.IsPossessing) param.PossessInvoker.LeaveCharacter(param.PlayerController);

                    // 取得
                    param.PossessInvoker.PossessCharacter(param.This);

                    if (!SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasObtainedAnima)
                    {
                        SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasObtainedAnima = true;
                    }

                    param.PossessInvoker.PossessCharacter_ShowLogIfFirstTime(param.This);

                   
                    LogManager.Instance.ShowAutomatically(
                        ZString.Format("{0} のアニマを取得した", GetName(param.This.characterType))
                    );

                    // タイマーは AnimalLeaveInvoker 側で一元管理してリセットする
                    var token = param.PossessInvoker.ResetPossessTimer(ct);

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
                        token);
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

        

        private static string GetName(CharacterType type) => type switch
        {
            CharacterType.Land => "陸",
            CharacterType.Sea => "海",
            CharacterType.Sky => "空",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
