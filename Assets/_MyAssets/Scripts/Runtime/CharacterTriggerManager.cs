using Cinemachine;

namespace MyScripts.Runtime
{
    internal sealed class CharacterTriggerManager : MonoBehaviour
    {
        private enum CharacterType : byte
        {
            Human,
            Dog, // 孵卵臭を認知
            Shell, // 汚染水を認知
        }

        [SerializeField] private Transform playerTransform; // プレイヤーのTransform
        [SerializeField] private Transform humanCapsule;
        [SerializeField] private Transform dogCapsule;
        [SerializeField] private Transform shellCapsule;
        [SerializeField] private CinemachineBrain playerCameraBrain;
        [SerializeField] private ParticleSystem[] sosSign_rottenEggSmell;
        [SerializeField] private ParticleSystem[] sosSign_contaminatedWater;
        [SerializeField] private TextMeshProUGUI triggerText; // トリガーを教えるUI
        [SerializeField] private TextMeshProUGUI triggerCtLabel;
        [SerializeField] private SOSSignFindManager sosSignFindManager;
        [SerializeField] private TimeScoreManager timeScoreManager;
        [SerializeField, Range(0.0f, 5.0f)] private float onTeleportCameraBlendFlowDuration = 0.5f;
        [SerializeField, Range(0.0f, 5.0f)] private float onTeleportCameraBlendAimDuration = 0.3f;

        // Awake で初期化
        private CharacterType currentType; // 現在のキャラクターの種類
        private Dictionary<CharacterType, Transform> characterCapsules; // 各キャラクターの最新座標を保持 (ワールド座標)
        private Dictionary<CharacterType, ParticleSystem[]> sosSigns; // 各キャラクターが認知できるSOSサイン

        private bool onTriggerCt = false;

        private static readonly Dictionary<CharacterType, string> characterNames = new()
        {
            { CharacterType.Human, "人間" },
            { CharacterType.Dog, "犬" },
            { CharacterType.Shell, "貝" }
        };

        private void Awake()
        {
            currentType = CharacterType.Human;

            characterCapsules = new()
            {
                { CharacterType.Human, humanCapsule },
                { CharacterType.Dog, dogCapsule },
                { CharacterType.Shell, shellCapsule }
            };

            sosSigns = new()
            {
                { CharacterType.Human, Array.Empty<ParticleSystem>() },
                { CharacterType.Dog, sosSign_rottenEggSmell },
                { CharacterType.Shell, sosSign_contaminatedWater }
            };

            // プレイヤーを人間のカプセルの所に移動させる
            playerTransform.SetPositionAndRotation(humanCapsule.position, humanCapsule.rotation);
            // 人間のカプセルは非表示
            humanCapsule.gameObject.SetActive(false);
            // SOSサインの可視性を初期化
            UpdateSOSSignsVisibility(currentType);
            // トリガーUIを更新
            triggerCtLabel.enabled = false;
            UpdateTriggerText(currentType, GetNext(currentType));

            {
                List<Collider> sosSignColliders = new(64);
                foreach (var kv in sosSigns)
                {
                    foreach (var v in kv.Value)
                    {
                        if (!v.transform.parent.TryGetComponent(out Collider c)) continue;
                        sosSignColliders.Add(c);
                    }
                }

                sosSignFindManager.Setup(
                    sosSignColliders.AsReadOnly(),
                    () => currentType == CharacterType.Human,
                    timeScoreManager.DecrementLeftAmount
                );
            }

            WaitInputAndTriggerAsync(destroyCancellationToken).Forget();
        }

        private static CharacterType GetNext(CharacterType type) => type switch
        {
            CharacterType.Human => CharacterType.Dog,
            CharacterType.Dog => CharacterType.Shell,
            CharacterType.Shell => CharacterType.Human,
            _ => type
        };

        private async UniTaskVoid WaitInputAndTriggerAsync(Ct ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // 人間 → 犬 → 貝 → 人間
                await UniTask.WaitUntil(() => !onTriggerCt && InputManager.Instance.InGameTriggerCharacter.Bool,
                    timing: PlayerLoopTiming.Update, cancellationToken: ct);
                // クールタイム中にする(切り替え処理の最後に、falseに戻す)
                onTriggerCt = true;
                if (triggerCtLabel != null)
                    triggerCtLabel.enabled = true;

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
                playerTransform.position,
                playerTransform.rotation
            );
            // カプセルをアクティブ化
            characterCapsules[from].gameObject.SetActive(true);

            // キャラクターの種類を切り替え
            currentType = to;

            // 切り替わり後のカプセルを非アクティブ化
            characterCapsules[to].gameObject.SetActive(false);
            // カメラの追尾を切る
            playerCameraBrain.enabled = false;
            // 切り替わり後のキャラクターの座標にテレポート
            playerTransform.SetPositionAndRotation(
                characterCapsules[to].position,
                characterCapsules[to].rotation
            );
            // カメラのブレンド演出開始
            DOCameraBlendAsync(
                characterCapsules[to].position,
                characterCapsules[to].rotation,
                destroyCancellationToken
            ).Forget();

            // SOSサインの可視性を更新
            UpdateSOSSignsVisibility(to);

            // トリガーUIを更新
            UpdateTriggerText(to, GetNext(to));
        }

        private async UniTaskVoid DOCameraBlendAsync(Vector3 toPosition, Quaternion toRotation, Ct ct)
        {
            float flowDur = this.onTeleportCameraBlendFlowDuration;
            float aimDur = this.onTeleportCameraBlendAimDuration;

            Vector3 flowEndPosition = playerCameraBrain.transform.position + Vector3.up * 20.0f;
            await playerCameraBrain.transform.DOMove(flowEndPosition, flowDur)
                .OnUpdate(() =>
                {
                    Vector3 direction = toPosition - playerCameraBrain.transform.position;
                    if (direction != Vector3.zero)
                        playerCameraBrain.transform.rotation = Quaternion.Lerp(
                            playerCameraBrain.transform.rotation,
                            Quaternion.LookRotation(direction),
                            Time.deltaTime * 6.0f
                        );
                })
                .WithCancellation(ct);

            await playerCameraBrain.transform.DOMove(toPosition, aimDur)
                .SetEase(Ease.InCubic)
                .OnComplete(() => playerCameraBrain.transform.rotation = toRotation)
                .WithCancellation(ct);

            // カメラの追尾を再開
            playerCameraBrain.enabled = true;

            // 切り替えのクールタイムを終了とする
            if (triggerCtLabel != null)
                triggerCtLabel.enabled = false;
            onTriggerCt = false;
        }

        private void UpdateSOSSignsVisibility(CharacterType type)
        {
            foreach (var kv in sosSigns)
            {
                bool isVisible = kv.Key == type;

                // 配列はnullでない想定
                foreach (var sosSign in kv.Value)
                {
                    if (sosSign != null)
                        sosSign.gameObject.SetActive(isVisible);
                }
            }
        }

        private void UpdateTriggerText(CharacterType now, CharacterType next)
            => triggerText.text =
                $$"""
                あなたは現在：{{characterNames[now]}}
                Fキーを押して {{characterNames[next]}} に切り替わる
                """;
    }
}
