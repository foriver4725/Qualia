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

        // Awake で初期化される
        private Camera playerCamera = null;

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
            // 必要なコンポーネントを動的に取得する
            AnimaDynamicComponents dynamicComponents = AnimaDynamicComponents.Instance;
            playerCamera = dynamicComponents.PlayerCamera;
            PlayerController pc = dynamicComponents.Pc;
            AnimalLeaveInvoker animalLeaveInvoker = dynamicComponents.AnimalLeaveInvoker;
            SOSSoundPlayer soundPlayer = dynamicComponents.SoundPlayer;

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
                .Subscribe(static param =>
                {
                    // 接触したら即取得。同種別スロットが取得済みなら PossessCharacter 内で上書き解放されるため
                    // ここでは事前解放不要（PossessCharacter が LeaveCharacterInternal を呼ぶ）
                    param.PossessInvoker.PossessCharacter(param.This);

                    // セーブデータ更新
                    if (SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasObtainedAnima == false)
                    {
                        SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].HasObtainedAnima = true;
                    }

                    param.PossessInvoker.PossessCharacter_ShowLogIfFirstTime(param.This);

                    LogManager.Instance.ShowAutomatically(ZString.Format("{0} のアニマを取得した",
                        GetName(param.This.characterType)));

                    // タイマーは AnimalLeaveInvoker 側でスロットごとに管理してリセットする
                    var token = param.PossessInvoker.ResetPossessTimer(
                        param.This.destroyCancellationToken, param.This.characterType);

                    UniTask.Void(async token =>
                        {
                            var type = param.This.characterType;
                            param.PossessInvoker.SetDisplayImageFillAmount(type, 1.0f);

                            float duration = param.This.sAnima.PossessDuration;
                            float t = duration;

                            while (t > 0.0f)
                            {
                                await UniTask.NextFrame(cancellationToken: token);
                                t -= Time.deltaTime;
                                param.PossessInvoker.SetDisplayImageFillAmount(type, t / duration);
                            }

                            param.PossessInvoker.SetDisplayImageFillAmount(type, 0.0f);
                            param.PossessInvoker.LeaveCharacter(param.PlayerController, type);
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
                CharacterType.Sea  => seaMaterial,
                CharacterType.Sky  => skyMaterial,
                _                  => null
            };

            icon.sprite = type switch
            {
                CharacterType.Land => sAnima.LandIcon,
                CharacterType.Sea  => sAnima.SeaIcon,
                CharacterType.Sky  => sAnima.SkyIcon,
                _                  => null
            };
        }

        private static string GetName(CharacterType type) => type switch
        {
            CharacterType.Land => "草",
            CharacterType.Sea  => "水",
            CharacterType.Sky  => "空",
            _                  => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}