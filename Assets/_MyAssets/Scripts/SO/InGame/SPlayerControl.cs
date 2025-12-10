namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_PlayerControl", menuName = "SO/InGame/Player Control")]
    internal sealed class SPlayerControl : ScriptableObject
    {
        // プレイヤーの移動入力を鈍感にするタイミング
        internal enum MoveInputInsensitiveTimingType : byte
        {
            Never = 0,
            WhileInAir = 1,
            WhileInAirAndWhenOuterVelocityIsNotZero = 2,
        }

        [Header("Move")]
        [SerializeField] private float moveSpeed = 4.0f;
        [SerializeField] private float sprintSpeedMultiplier = 1.5f;
        [SerializeField] private float moveSpeedMultiplierWhenHasLand = 1.5f;
        [SerializeField] private float moveSpeedMultiplierWhenHasSea = 3.0f;
        [SerializeField] private float moveAcceleration = 10.0f;
        [SerializeField, Tooltip("入力によらない水平移動速度の,地上での減衰係数 (空気抵抗に相当)")]
        private float nativeHorizontalVelocityAttenuationRateOnGround = 0.50f;
        [SerializeField, Tooltip("入力によらない水平移動速度の,空中での減衰係数 (空気抵抗に相当)")]
        private float nativeHorizontalVelocityAttenuationRateInAir = 0.02f;
        [SerializeField, Tooltip("移動入力を鈍感にするタイミング")]
        private MoveInputInsensitiveTimingType moveInputInsensitiveTiming = MoveInputInsensitiveTimingType.Never;
        [SerializeField, Tooltip("移動入力を鈍感にするとき,元の入力値の何倍にするか")] private float moveInputInsensitiveRate = 0.5f;
        internal float MoveSpeed => moveSpeed;
        internal float SprintSpeedMultiplier => sprintSpeedMultiplier;
        internal float MoveSpeedMultiplierWhenHasLand => moveSpeedMultiplierWhenHasLand;
        internal float MoveSpeedMultiplierWhenHasSea => moveSpeedMultiplierWhenHasSea;
        internal float MoveAcceleration => moveAcceleration;
        internal float NativeHorizontalVelocityAttenuationRateOnGround => nativeHorizontalVelocityAttenuationRateOnGround;
        internal float NativeHorizontalVelocityAttenuationRateInAir => nativeHorizontalVelocityAttenuationRateInAir;
        internal MoveInputInsensitiveTimingType MoveInputInsensitiveTiming => moveInputInsensitiveTiming;
        internal float MoveInputInsensitiveRate => moveInputInsensitiveRate;

        [Space(10)]

        [Header("Look")]
        [SerializeField] private float rotationSpeed = 1.0f;
        [SerializeField, MinMaxRange(-90.0f, 90.0f), Tooltip("カメラを上下に動かす角度の許容範囲 [度]")] private Vector2 cameraPitchRange = new(-90.0f, 90.0f);
        internal float RotationSpeed => rotationSpeed;
        internal float CameraPitchMin => cameraPitchRange.x;
        internal float CameraPitchMax => cameraPitchRange.y;

        [Space(10)]

        [Header("Jump")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField, Tooltip("プレイヤーは独自の重力を使う. エンジンのデフォルトは -9.81f.")] private float ownGravity = -15.0f;
        [SerializeField] private float ownGravityWhenHasSky = -2.0f;
        internal float JumpHeight => jumpHeight;
        internal float OwnGravity => ownGravity;
        internal float OwnGravityWhenHasSky => ownGravityWhenHasSky;

        [Space(10)]

        [Header("Inertia Jump")]
        [SerializeField, Tooltip("加算する速度(プレイヤーから見た相対ベクトル)")] private Vector3 inertiaJumpVelocity = new(30.0f, 15.0f, 30.0f);
        [SerializeField, Tooltip("必要な水平速度 の平方")] private float inertiaJumpLimitSpeedSqr = 100.0f;
        [SerializeField] private float inertiaJumpCoolTime = 0.2f;
        internal Vector3 InertiaJumpVelocity => inertiaJumpVelocity;
        internal float InertiaJumpLimitSpeedSqr => inertiaJumpLimitSpeedSqr;
        internal float InertiaJumpCoolTime => inertiaJumpCoolTime;

        [Space(10)]

        [Header("Timeouts")]
        [SerializeField, Tooltip("ジャンプ可能になるまでの時間. 0f に設定すると即座にジャンプ可能になる.")] private float jumpTimeout = 0.1f;
        [SerializeField, Tooltip("落下ステートに入るまでの時間. 階段を降りる状況で有用.")] private float fallTimeout = 0.15f;
        internal float JumpTimeout => jumpTimeout;
        internal float FallTimeout => fallTimeout;

        [Space(10)]

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField, Tooltip("探知の中心座標を,プレイヤーの足元から少し上にずらす値. 粗い地面の場合に有用.")] private float groundCheckOffset = 0.14f;
        [SerializeField, Tooltip("探知の半径. CharacterController の radius と一致させるべき.")] private float groundCheckRadius = 0.5f;
        internal LayerMask GroundLayers => groundLayers;
        internal float GroundCheckOffset => groundCheckOffset;
        internal float GroundCheckRadius => groundCheckRadius;

        [Space(10)]

        [Header("Camera Blend on Character Trigger")]
        [SerializeField] private CameraBlendSettingsOnCharacterTrigger cameraBlendOnCharacterTrigger;
        internal CameraBlendSettingsOnCharacterTrigger CameraBlendOnCharacterTrigger => cameraBlendOnCharacterTrigger;
        [Serializable]
        internal sealed class CameraBlendSettingsOnCharacterTrigger
        {
            [SerializeField, Range(0.0f, 5.0f), Tooltip("浮き上がりの所要時間 [s]")] private float floatDuration = 0.5f;
            [SerializeField, Range(0.0f, 50.0f), Tooltip("浮き上がりの高さ [m]")] private float floatHeight = 20.0f;
            [SerializeField, Range(0.0f, 20.0f), Tooltip("浮き上がり中、ターゲットを注視する回転速度 (係数)")] private float floatLookSpeed = 6.0f;
            [SerializeField, Range(0.0f, 1000.0f), Tooltip("移動速度 [m/s]")] private float moveSpeed = 500.0f;
            [SerializeField, MinMaxRange(0.0f, 5.0f), Tooltip("移動時間の許容範囲 [s]")] private Vector2 moveDurationRange = new(0.5f, 2.0f);
            [SerializeField, Range(0.0f, 5.0f), Tooltip("移動時間が 〇[s] より長いならば、移動終了時の効果音が鳴る")] private float moveDurationMinToPlayCloseToEndSound = 1.0f;
            internal float FloatDuration => floatDuration;
            internal float FloatHeight => floatHeight;
            internal float FloatLookSpeed => floatLookSpeed;
            internal float MoveSpeed => moveSpeed;
            internal float MoveDurationMin => moveDurationRange.x;
            internal float MoveDurationMax => moveDurationRange.y;
            internal float MoveDurationMinToPlayCloseToEndSound => moveDurationMinToPlayCloseToEndSound;
        }
    }
}
