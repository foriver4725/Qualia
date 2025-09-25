using Cinemachine;

namespace MyScripts.Runtime
{
    internal sealed class CharacterTriggerManager : MonoBehaviour
    {
        private enum CharacterType : byte
        {
            Human,
            Animal,
        }

        [SerializeField] private Transform playerTransform; // プレイヤーのTransform
        [SerializeField] private Transform humanCapsule;
        [SerializeField] private Transform animalCapsule;
        [SerializeField] private CinemachineBrain playerCameraBrain;
        [SerializeField] private Transform sosSignsRoot; // SOSサインの親オブジェクト (配下にはSOSサインしか置かない前提)
        [SerializeField] private TextMeshProUGUI triggerText; // トリガーを教えるUI
        [SerializeField] private TextMeshProUGUI triggerCtLabel;
        [SerializeField] private PlayerController pc;
        [SerializeField] private GameObject playerCapsule;
        [SerializeField] private SOSSignFindManager sosSignFindManager;
        [SerializeField] private TimeScoreManager timeScoreManager;
        [SerializeField] private CharacterTriggerSoundPlayer soundPlayer;

        // Awake で初期化
        private SPlayerControl.CameraBlendSettingsOnCharacterTrigger param;
        private ParticleSystem[] sosSigns;
        private CharacterType currentType; // 現在のキャラクターの種類
        private Dictionary<CharacterType, Transform> characterCapsules; // 各キャラクターの最新座標を保持 (ワールド座標)
        private Vector3 characterCapsuleLocalPosition; // キャラクターカプセルは、ルートからオフセットされている(足元を中心にするため)
        private int characterCapsuleInitLayer;
        private int CharacterOutlineLayer; // 定数

        private bool onTriggerCt = false;

        private static readonly Dictionary<CharacterType, string> characterNames = new()
        {
            { CharacterType.Human, "人間" },
            { CharacterType.Animal, "動物" },
        };

        private void Awake()
        {
            param = InGameSOHolder.Instance.PlayerControl.CameraBlendOnCharacterTrigger;
            sosSigns = sosSignsRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true);

            {
                currentType = CharacterType.Human;

                characterCapsules = new()
                {
                    { CharacterType.Human, humanCapsule },
                    { CharacterType.Animal, animalCapsule },
                };

                characterCapsuleLocalPosition = playerCapsule.transform.localPosition;
                characterCapsuleInitLayer = playerCapsule.layer;
                CharacterOutlineLayer = LayerMask.NameToLayer("CharacterOutline");

                // プレイヤーを人間のカプセルの所に移動させる
                playerTransform.SetPositionAndRotation(humanCapsule.position, humanCapsule.rotation);
                // 人間のカプセルは非表示
                humanCapsule.gameObject.SetActive(false);
                // SOSサインの可視性を初期化
                UpdateSOSSignsVisibility(currentType);
                // トリガーUIを更新
                triggerCtLabel.enabled = false;
                UpdateTriggerText(currentType, GetNext(currentType));
            }

            {
                Collider[] sosSignColliders = sosSignsRoot.GetComponentsInChildren<Collider>(includeInactive: true);

                sosSignFindManager.Setup(
                    Array.AsReadOnly(sosSignColliders),
                    () => currentType == CharacterType.Human,
                    timeScoreManager.DecrementLeftAmount
                );
            }

            WaitInputAndTriggerAsync(destroyCancellationToken).Forget();
        }

        private static CharacterType GetNext(CharacterType type) => type switch
        {
            CharacterType.Human => CharacterType.Animal,
            CharacterType.Animal => CharacterType.Human,
            _ => type
        };

        private async UniTaskVoid WaitInputAndTriggerAsync(Ct ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // 人間 → 犬 → 貝 → 人間
                await UniTask.WaitUntil(() => !onTriggerCt && InputManager.InGameTriggerCharacter.Bool,
                    timing: PlayerLoopTiming.Update, cancellationToken: ct);

                // クールタイム中にする(切り替え処理の最後に、falseに戻す)
                onTriggerCt = true;
                if (triggerCtLabel != null)
                    triggerCtLabel.enabled = true;

                // プレイヤーコントロールの入力を無効化(切り替え処理の最後に、trueに戻す)
                pc.IsPcInputEnabled = false;
                // プレイヤーに働く重力を無効化(切り替え処理の最後に、trueに戻す)
                pc.IsOwnGravityEnabled = false;

                // プレイヤー側の移動があるため、LateUpdateのタイミングまで待つ
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

                // キャラクターを切り替える
                Trigger(currentType, GetNext(currentType));
            }
        }

        private void Trigger(CharacterType from, CharacterType to)
        {
            // 辞書型のキーが必ず存在する前提

            // 切り替わり前の所にカプセルを残しておく
            characterCapsules[from].SetPositionAndRotation(
                playerTransform.position + characterCapsuleLocalPosition,
                playerTransform.rotation
            );
            // カプセルをアクティブ化
            // (カメラにアウトラインが映ってチラついてしまうため、カメラのブレンド演出が始まった後少しだけ経過してから、アクティブにする)
            0.05f.SecAwaitThenDo(() => characterCapsules[from].gameObject.SetActive(true), ct: destroyCancellationToken).Forget();

            // キャラクターの種類を切り替え
            currentType = to;

            // 切り替わり後のカプセルを非アクティブ化
            characterCapsules[to].gameObject.SetActive(false);
            // カメラの追尾を切る
            playerCameraBrain.enabled = false;
            // プレイヤーカプセルにアウトラインを付ける
            playerCapsule.layer = CharacterOutlineLayer;
            // 切り替わり後は移動した方向が正面になるように回転するので、その回転をここで計算
            Vector3 targetPosition = characterCapsules[to].position - characterCapsuleLocalPosition;
            Vector3 targetDirection = targetPosition - playerTransform.position;
            Vector3 targetDirectionXZ = new(targetDirection.x, 0.0f, targetDirection.z);
            Quaternion targetRotation = Quaternion.LookRotation(targetDirectionXZ, Vector3.up);
            // 切り替わり後のキャラクターの座標にテレポート
            playerTransform.SetPositionAndRotation(
                targetPosition,
                targetRotation
            );
            // カメラのブレンド演出開始
            DOCameraBlendAsync(
                targetPosition,
                targetRotation,
                destroyCancellationToken
            ).Forget();

            // SOSサインの可視性を更新
            UpdateSOSSignsVisibility(to);

            // トリガーUIを更新
            UpdateTriggerText(to, GetNext(to));
        }

        private async UniTaskVoid DOCameraBlendAsync(Vector3 toPosition, Quaternion toRotation, Ct ct)
        {
            Vector3 moveDirection = toPosition - playerCameraBrain.transform.position;
            float moveDuration = moveDirection.magnitude / param.MoveSpeed;
            moveDuration = Mathf.Clamp(moveDuration, param.MoveDurationMin, param.MoveDurationMax);

            soundPlayer.LetPlay(SCharacterTriggerSound.Timing.Begin);

            await playerCameraBrain.transform.DOMove(
                playerCameraBrain.transform.position + Vector3.up * param.FloatHeight,
                param.FloatDuration
            )
                .OnUpdate(() =>
                {
                    Vector3 directionCurrent = toPosition - playerCameraBrain.transform.position;
                    if (directionCurrent != Vector3.zero)
                        playerCameraBrain.transform.rotation = Quaternion.Lerp(
                            playerCameraBrain.transform.rotation,
                            Quaternion.LookRotation(directionCurrent),
                            Time.deltaTime * param.FloatLookSpeed
                        );
                })
                .WithCancellation(ct);

            if (moveDuration > param.MoveDurationMinToPlayCloseToEndSound)
            {
                float letPlayTime = moveDuration - soundPlayer.CloseToEndPlayLength;
                letPlayTime.SecAwaitThenDo(() => soundPlayer.LetPlay(SCharacterTriggerSound.Timing.CloseToEnd), ct: ct).Forget();
            }

            await playerCameraBrain.transform.DOMove(toPosition, moveDuration)
                .SetEase(Ease.InCubic)
                .OnComplete(() => playerCameraBrain.transform.rotation = toRotation)
                .WithCancellation(ct);

            // プレイヤーカプセルのアウトラインを外す
            playerCapsule.layer = characterCapsuleInitLayer;

            // カメラの追尾を再開
            playerCameraBrain.enabled = true;

            // プレイヤーに働く重力を有効化
            pc.IsOwnGravityEnabled = true;
            // プレイヤーコントロールの入力を有効化
            pc.IsPcInputEnabled = true;

            // 切り替えのクールタイムを終了とする
            if (triggerCtLabel != null)
                triggerCtLabel.enabled = false;
            onTriggerCt = false;
        }

        private void UpdateSOSSignsVisibility(CharacterType type)
        {
            bool isVisible = (type != CharacterType.Human);

            foreach (var sign in sosSigns)
            {
                if (sign != null)
                    sign.gameObject.SetActive(isVisible);
            }
        }

        private void UpdateTriggerText(CharacterType now, CharacterType next)
            => triggerText.SetTextFormat("あなたは現在：{0}\nFキーを押して {1} に切り替わる",
                characterNames[now], characterNames[next]);
    }
}
