using MyScripts.Common.SaveSystem;

namespace MyScripts.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    internal sealed class PlayerController : MonoBehaviour, IDataHoldingObject
    {
        [Serializable]
        private sealed class WalkSoundBorderRoots
        {
            [SerializeField] private GameObject grass;
            [SerializeField] private GameObject sand;
            [SerializeField] private GameObject rock;
            [SerializeField] private GameObject water;

            internal GameObject Get(SWalkSound.Surface surface) => surface switch
            {
                SWalkSound.Surface.Grass => grass,
                SWalkSound.Surface.Sand  => sand,
                SWalkSound.Surface.Rock  => rock,
                SWalkSound.Surface.Water => water,
                _                        => throw new ArgumentOutOfRangeException(nameof(surface), surface, null)
            };
        }

        // @formatter:off
        [Header("Player Control")]
        [SerializeField] private CharacterController controller;
        [SerializeField] private AnimalLeaveInvoker animalLeaveInvoker;
        [SerializeField] private CameraFOVManager cameraFOVManager;
        [SerializeField] private PlayerControlSoundPlayer soundPlayer;
        [SerializeField] private Transform cinemachineCameraTarget;
        [SerializeField] private Transform teleportBackPoint;
        [Space(10)]
        [Header("Walk Sound")]
        [SerializeField] private WalkSoundPlayer walkSoundPlayer;
        [SerializeField] private WalkSoundBorderRoots walkSoundBorderRoots;
        // @formatter:on

        // cinemachine
        private float cinemachineTargetPitch;

        // player
        private float rotationVelocity;
        private Vector2 nativeHorizontalVelocity = Vector2.zero; // 入力によらない水平移動速度 (段々減衰していき,0になる)
        private Vector3 realHorizontalVelocity;
        private float verticalVelocity;
        private static readonly float TerminalVelocity = 53.0f;
        private bool isGrounded = true;
        private bool hasBecameGroundedThisFrame = false;
        private bool hasBecameNotGroundedThisFrame = false;
        private Vector3 becameGroundedPosition = Vector3.zero;    // 一番最後に地面に着地した位置を記録しておく
        private Vector3 becameNotGroundedPosition = Vector3.zero; // 一番最後に地面から離れた位置を記録しておく
        private bool isJumping = false;
        private bool isSprinting = false;
        private bool isDoingInertiaJump = false;
        private bool onInertiaJumpCt = false;
        private Vector2 appliedHorizontalOuterVelocityWhenInertiaJumpBeganLast = Vector2.zero; // 直近の慣性ジャンプで加算した水平速度
        private float horizontalVelocityTotalAttenuatedRateSinceInertiaJumpBegan = 0.0f; // 慣性ジャンプ開始以降、トータルの水平速度 減衰割合
        private Vector3 previousFramePosition = Vector3.zero; // 直前フレームの位置を記録して、戻せるようにする
        private int jumpCountWhenHasSky = 0; // 空のアニマを取得している時、空中ジャンプ出来るので、二段ジャンプより上を防止する用

        // timeout deltatime
        // Awake で初期化
        private SPlayerControl param;
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;

        // constraints
        private bool isOwnGravityEnabled = true;

        internal bool IsOwnGravityEnabled
        {
            get => isOwnGravityEnabled;
            set
            {
                isOwnGravityEnabled = value;

                // 重力を無効化する時、各種速度(入力無効で0にならないもの)をリセットする
                if (!value)
                {
                    nativeHorizontalVelocity = Vector2.zero;
                    verticalVelocity = 0.0f;
                }
            }
        }

        internal bool CanApplyVelocityDelta { get; set; } = true;

        // walk sound
        private byte walkSoundUpdateIntervalFrames; // Awake で初期化

        private byte walkSoundUpdateFrameCounter = 0;

        // 最初の方 (= 上の地層にある地面) を優先して鳴らす
        private static readonly ReadOnlyCollection<SWalkSound.Surface> WalkSoundPriority = Array.AsReadOnly(
            new SWalkSound.Surface[]
            {
                SWalkSound.Surface.Rock,
                SWalkSound.Surface.Water,
                SWalkSound.Surface.Sand,
                SWalkSound.Surface.Grass,
            });

        private readonly Dictionary<SWalkSound.Surface, ReadOnlyCollection<Border>>
            walkSoundBorders = new(); // Awake で初期化

        #region Public Methods and Properties

        internal Collider Collider => controller;
        internal bool IsGrounded => isGrounded;

        internal void Teleport(Vector3 position, Vector3 forward)
        {
            // 一応、切り替わり中の挙動制限も適用しておく
            InputManager.PlayerControl.Enabled = false;
            IsOwnGravityEnabled = false;
            CanApplyVelocityDelta = false;

            {
                transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));
            }

            CanApplyVelocityDelta = true;
            IsOwnGravityEnabled = true;
            InputManager.PlayerControl.Enabled = true;
        }

        #endregion

        #region Interface Implementation

        public void GetDataAndUpdateMyProperties()
        {
            Vector3 playerPosition = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].PlayerPosition;
            Vector3 playerForward = SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].PlayerForward;
            Teleport(playerPosition, playerForward);
        }

        public void SetMyPropertiesToData()
        {
            // ジャンプ中ならダメ
            if (isJumping) return;

            // 慣性ジャンプ中ならダメ
            if (isDoingInertiaJump) return;

            Vector3 playerPosition = transform.position;
            Vector3 playerForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized; // Y成分は無視する
            SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].PlayerPosition = playerPosition;
            SaveLoadManager.Data.Slots[Variables.CurrentSlotIndex].PlayerForward = playerForward;
        }

        #endregion

        private void Awake()
        {
            param = InGameSOHolder.Instance.PlayerControl;

            // reset our timeouts on start
            jumpTimeoutDelta = param.JumpTimeout;
            fallTimeoutDelta = param.FallTimeout;

            walkSoundUpdateIntervalFrames = InGameSOHolder.Instance.GameParameter.WalkSoundUpdateIntervalFrames;

            // walkSoundBorders
            foreach (var surface in WalkSoundPriority)
            {
                var borders = walkSoundBorderRoots.Get(surface).GetComponentsInChildren<Border>(includeInactive: true);
                walkSoundBorders.Add(surface, Array.AsReadOnly(borders));
            }

            GetDataAndUpdateMyProperties();
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            DoInertiaJumpIfTheTiming();
            StopInertiaJumpWhenSprintEnd();
            AttenuateNativeHorizontalVelocity();
            InputAndFinallyMove();
            TeleportBackWhenInvalidPosition();

            UpdateFOVsSprintMode();
            UpdateWalkSound();

            RecordPreviousFramePosition();
            SetMyPropertiesToData();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        // 入力・重力によるものではない、外力による速度増加
        // 速度ベクトルに一回のみ加算するため、加算した分の影響は、段々減衰する
        // プレイヤーに衝撃を与えるときなどに使う
        private void ApplyOuterVelocity(Vector3 velocity)
        {
            nativeHorizontalVelocity += new Vector2(velocity.x, velocity.z);
            verticalVelocity += velocity.y;
        }

        /// <summary>
        /// 毎フレーム、速度ベクトルに加算される外力ベクトル
        /// 減衰しない、毎フレーム一定値の値
        /// プレイヤーを押すときなどに使う
        /// </summary>
        internal Vector3 VelocityDelta { get; set; } = Vector3.zero;

        private void GroundedCheck()
        {
            bool isGroundedPrev = isGrounded;

            // set sphere position, with offset
            Vector3 spherePosition = new(transform.position.x, transform.position.y + param.GroundCheckOffset,
                transform.position.z);
            isGrounded = Physics.CheckSphere(spherePosition, param.GroundCheckRadius, param.GroundLayers,
                QueryTriggerInteraction.Ignore);

            hasBecameGroundedThisFrame = isGrounded && !isGroundedPrev;
            hasBecameNotGroundedThisFrame = !isGrounded && isGroundedPrev;

            // 着地・離地位置を記録
            if (hasBecameGroundedThisFrame)
                becameGroundedPosition = transform.position;
            if (hasBecameNotGroundedThisFrame)
                becameNotGroundedPosition = transform.position;

            // 地面に着地したら、少しだけ(クールタイム)待ってから、再度慣性ジャンプを行えるようにする
            if (isGrounded && isDoingInertiaJump && !onInertiaJumpCt)
            {
                onInertiaJumpCt = true;

                param.InertiaJumpCoolTime.SecAwaitThenDo(() =>
                {
                    isDoingInertiaJump = false;
                    onInertiaJumpCt = false;
                }, ct: destroyCancellationToken).Forget();
            }
        }

        // 慣性ジャンプ
        private void DoInertiaJumpIfTheTiming()
        {
            // 陸のアニマを取得していないとダメ
            if (!animalLeaveInvoker.IsPossessingType(CharacterType.Land)) return;

            // 水平方向にある程度の速度が必要
            if (realHorizontalVelocity.sqrMagnitude < param.InertiaJumpLimitSpeedSqr) return;

            // ダッシュしていないならダメ
            if (!isSprinting) return;

            // ジャンプ中ならダメ
            if (isJumping) return;

            // 慣性ジャンプ中ならダメ
            if (isDoingInertiaJump) return;

            // 現在、地面から離れたタイミングであるべき
            if (!hasBecameNotGroundedThisFrame) return;

            // 目の前が崖であるべき
            {
                if (!TryMeasureCliffHeight(param.InertiaJumpCliffCheckDistanceFromPlayerNear, out float heightNear))
                    return; // 検出失敗
                if (!TryMeasureCliffHeight(param.InertiaJumpCliffCheckDistanceFromPlayerFar, out float heightFar))
                    return; // 検出失敗

                if (heightFar - heightNear > param.InertiaJumpCliffCheckHeightDifferenceLimit)
                    return;
            }

            // 処理を行える

            isDoingInertiaJump = true;

            Vector2 moveDirectionXZ = new Vector2(realHorizontalVelocity.x, realHorizontalVelocity.z).normalized;
            Vector3 velocity = new(
                moveDirectionXZ.x * param.InertiaJumpVelocity.x,
                param.InertiaJumpVelocity.y,
                moveDirectionXZ.y * param.InertiaJumpVelocity.z
            );

            appliedHorizontalOuterVelocityWhenInertiaJumpBeganLast = new Vector2(velocity.x, velocity.z);
            horizontalVelocityTotalAttenuatedRateSinceInertiaJumpBegan = 1.0f;
            ApplyOuterVelocity(velocity);

            soundPlayer.LetPlay(SPlayerControlSound.Action.InertiaJump);
        }

        // 慣性ジャンプ 強制中止
        private void StopInertiaJumpWhenSprintEnd()
        {
            // 慣性ジャンプ中のみ
            if (!isDoingInertiaJump) return;

            // ダッシュしていないなら
            if (isSprinting) return;

            // 慣性ジャンプによる速度増加を打ち消す

            Vector2 cancelHorizontalVelocity = appliedHorizontalOuterVelocityWhenInertiaJumpBeganLast *
                                               horizontalVelocityTotalAttenuatedRateSinceInertiaJumpBegan;
            ApplyOuterVelocity(new Vector3(-cancelHorizontalVelocity.x, 0.0f, -cancelHorizontalVelocity.y));

            appliedHorizontalOuterVelocityWhenInertiaJumpBeganLast = Vector2.zero;
            horizontalVelocityTotalAttenuatedRateSinceInertiaJumpBegan = 0.0f;

            isDoingInertiaJump = false;
        }

        private void CameraRotation()
        {
            // get input
            Vector2 input = InputManager.PlayerControl.Look;

            // if there is an input
            if (input.sqrMagnitude >= 0.01f)
            {
                //Don't multiply mouse input by Time.deltaTime
                bool isCurrentDeviceMouse = InputManager.GetCurrentDevice() == InputManager.Device.KeyboardAndMouse;
                float deltaTimeMultiplier = isCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                cinemachineTargetPitch += input.y * param.RotationSpeed * deltaTimeMultiplier;
                rotationVelocity = input.x * param.RotationSpeed * deltaTimeMultiplier;

                // clamp our pitch rotation
                cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, param.CameraPitchMin, param.CameraPitchMax);

                // Update Cinemachine camera target pitch
                cinemachineCameraTarget.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * rotationVelocity);
            }
        }

        private void AttenuateNativeHorizontalVelocity()
        {
            // attenuate the native velocity
            if (nativeHorizontalVelocity != Vector2.zero)
            {
                Vector2 originalNativeHorizontalVelocity = nativeHorizontalVelocity;

                float attenuationRate = isGrounded
                    ? param.NativeHorizontalVelocityAttenuationRateOnGround
                    : param.NativeHorizontalVelocityAttenuationRateInAir;
                nativeHorizontalVelocity -= nativeHorizontalVelocity * attenuationRate;

                if (nativeHorizontalVelocity.sqrMagnitude < 1e-4f)
                {
                    nativeHorizontalVelocity = Vector2.zero;
                }

                // update total attenuation record
                if (horizontalVelocityTotalAttenuatedRateSinceInertiaJumpBegan > 0.0f)
                {
                    float attenuatedRate =
                        nativeHorizontalVelocity.magnitude / originalNativeHorizontalVelocity.magnitude;
                    horizontalVelocityTotalAttenuatedRateSinceInertiaJumpBegan *= attenuatedRate;
                }
            }
        }

        private void InputAndFinallyMove()
        {
            // get input
            Vector2 input = InputManager.PlayerControl.Move;
            Vector3 inputDirection = new Vector3(input.x, 0.0f, input.y).normalized; // normalise input direction
            bool isInputSpecifiedAngle = false;                                      //入力方向と体のなす角が走行時の視野角に収まっているか
            bool isSprintingInput = InputManager.PlayerControl.Sprint;
            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            bool hasInput = input != Vector2.zero;
            if (hasInput)
            {
                inputDirection = transform.right * input.x + transform.forward * input.y;
                isInputSpecifiedAngle = IsMovingForward(inputDirection);
            }

            inputDirection.Normalize();
            isSprinting = isSprintingInput && hasInput && isInputSpecifiedAngle; //ここに接地条件を入れてもよいかもしれない

            // set target speed based on move speed, and sprint speed if sprint is pressed
            // when player is possessing an anima, increase move speed accordingly
            float targetSpeed = param.MoveSpeed;
            if (isSprinting)
                targetSpeed += param.SprintSpeedIncrease;
            // 特定のアニマを取得している場合、対応するエリア内で移動速度が速くなる
            if (animalLeaveInvoker.IsPossessingType(CharacterType.Sea))
            {
                if ((isGrounded && IsInsideOfArea(SWalkSound.Surface.Water, controller.transform.position)) ||
                    (!isGrounded && IsInsideOfArea(SWalkSound.Surface.Water, becameGroundedPosition)))
                {
                    targetSpeed += param.MoveSpeedIncreaseWhenHasSea;
                }
            }

            if (animalLeaveInvoker.IsPossessingType(CharacterType.Land))
            {
                // Water 以外の場所を陸上とみなす
                if ((isGrounded && !IsInsideOfArea(SWalkSound.Surface.Water, controller.transform.position)) ||
                    (!isGrounded && !IsInsideOfArea(SWalkSound.Surface.Water, becameGroundedPosition)))
                {
                    targetSpeed += param.MoveSpeedIncreaseWhenHasLand;
                }
            }

            if (animalLeaveInvoker.IsPossessingType(CharacterType.Sky))
            {
                if (jumpCountWhenHasSky > 0) targetSpeed += param.MoveSpeedIncreaseWhenHasSkyAndInTheAir;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // for debug, make the player move faster while has the input
            if (InputManager.Debug.FastenMoveSpeed)
                targetSpeed *= 5.0f;
#endif

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // if there is no input, set the target speed to 0
            if (!hasInput) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            // not using analog input
#if false
			float inputMagnitude = isAnalogInput ? input.magnitude : 1f;
#else
            float inputMagnitude = 1f;
#endif

            // accelerate or decelerate to target speed
            float speed;
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * param.MoveAcceleration);

                // round speed to 3 decimal places
                speed = Mathf.Round(speed * 1000f) / 1000f;
            }
            else
            {
                speed = targetSpeed;
            }

            // when it is the timing, make move input insensitive
            if (param.MoveInputInsensitiveTiming == SPlayerControl.MoveInputInsensitiveTimingType.WhileInAir)
            {
                if (!isGrounded)
                {
                    inputDirection *= param.MoveInputInsensitiveRate;
                }
            }
            else if (param.MoveInputInsensitiveTiming ==
                     SPlayerControl.MoveInputInsensitiveTimingType.WhileInAirAndWhenOuterVelocityIsNotZero)
            {
                if (!isGrounded && nativeHorizontalVelocity != Vector2.zero)
                {
                    inputDirection *= param.MoveInputInsensitiveRate;
                }
            }

            // calculate the real velocity
            realHorizontalVelocity = inputDirection * speed +
                                     new Vector3(nativeHorizontalVelocity.x, 0.0f, nativeHorizontalVelocity.y);
            Vector3 realVelocity = realHorizontalVelocity + new Vector3(0.0f, verticalVelocity, 0.0f);

            // 外力による速度増加分を加算
            if (CanApplyVelocityDelta)
                realVelocity += VelocityDelta;

            // move the player
            controller.Move(realVelocity * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (isGrounded)
            {
                // 空のアニマを取得している時の、二段ジャンプ防止用カウンタをリセット
                if (jumpCountWhenHasSky > 0) // 一応条件分岐
                    jumpCountWhenHasSky = 0;

                // get input
                bool input = InputManager.PlayerControl.Jump;

                // reset the fall timeout timer
                fallTimeoutDelta = param.FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (verticalVelocity < 0.0f)
                {
                    verticalVelocity = -2f;
                }

                // Jump
                if (input && jumpTimeoutDelta <= 0.0f)
                {
                    isJumping = true;

                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    verticalVelocity = Mathf.Sqrt(param.JumpHeight * -2f * param.OwnGravity);
                }

                // jump timeout
                if (jumpTimeoutDelta >= 0.0f)
                {
                    jumpTimeoutDelta -= Time.deltaTime;

                    // ここでジャンプが終了したとみなす
                    if (jumpTimeoutDelta <= 0.0f)
                    {
                        isJumping = false;
                    }
                }
            }
            // 空中ジャンプ (空のアニマを取得している時)
            // 通常のジャンプとほぼ同じロジック、ただしジャンプ力がめっちゃ強い
            // 今たまたまロジックが共通しているだけなので、一緒の関数にまとめたりなどはしない
            else if (animalLeaveInvoker.IsPossessingType(CharacterType.Sky))
            {
                // 既にジャンプ済みでないか?
                if (jumpCountWhenHasSky <= 0)
                {
                    // get input
                    bool input = InputManager.PlayerControl.Jump;

                    // reset the fall timeout timer
                    fallTimeoutDelta = param.FallTimeout;

                    // stop our velocity dropping infinitely when grounded
                    if (verticalVelocity < 0.0f)
                    {
                        verticalVelocity = -2f;
                    }

                    // Jump
                    if (input && jumpTimeoutDelta <= 0.0f)
                    {
                        isJumping = true;

                        // 二段ジャンプのカウンタを増やす
                        jumpCountWhenHasSky++;

                        // the square root of H * -2 * G = how much velocity needed to reach desired height
                        verticalVelocity = Mathf.Sqrt(param.JumpHeightWhenHasSkyAndInTheAir * -2f * param.OwnGravity);
                    }
                }

                // jump timeout
                if (jumpTimeoutDelta >= 0.0f)
                {
                    jumpTimeoutDelta -= Time.deltaTime;

                    // ここでジャンプが終了したとみなす
                    if (jumpTimeoutDelta <= 0.0f)
                    {
                        isJumping = false;
                    }
                }
            }
            else
            {
                // reset the jump timeout timer
                jumpTimeoutDelta = param.JumpTimeout;

                // fall timeout
                if (fallTimeoutDelta >= 0.0f)
                {
                    fallTimeoutDelta -= Time.deltaTime;
                }
            }

            if (IsOwnGravityEnabled)
            {
                // adjust gravity when possessing an anima
                float ownGravity = param.OwnGravity;
                if (animalLeaveInvoker.IsPossessingType(CharacterType.Sky))
                {
                    // 下向きに落下しているなら
                    if (verticalVelocity < 0.0f)
                        ownGravity = param.OwnGravityWhenHasSkyAndIsFalling;
                }

                // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
                if (verticalVelocity < TerminalVelocity)
                {
                    verticalVelocity += ownGravity * Time.deltaTime;
                }
            }
        }

        // 不正な場所にいる時、初期位置に戻す or 直前フレームの位置に戻す
        private void TeleportBackWhenInvalidPosition()
        {
            // ゲーム世界の範囲外か?
            // 初期位置に戻す
            if (controller.transform.position is
                { x: < -1600 or > 1600 } or
                { y: < -50 or > 500 } or
                { z: < -1600 or > 1600 })
                controller.transform.position = teleportBackPoint.position;
        }

        private void UpdateFOVsSprintMode()
        {
            // 重複して呼んでもOKなので、毎フレーム呼んでしまう
            if (isSprinting)
                cameraFOVManager.AddMode(CameraFOVManager.Mode.OnSprint);
            else
                cameraFOVManager.RemoveMode(CameraFOVManager.Mode.OnSprint);
        }

        private void UpdateWalkSound()
        {
            walkSoundUpdateFrameCounter++;
            if (walkSoundUpdateFrameCounter < walkSoundUpdateIntervalFrames) return;
            walkSoundUpdateFrameCounter = 0;

            var surface = GetSurfaceUnderfoot();
            walkSoundPlayer.LetPlay(surface, new() { IsSprinting = isSprinting });
        }

        private void RecordPreviousFramePosition()
        {
            previousFramePosition = controller.transform.position;
        }

        private SWalkSound.Surface GetSurfaceUnderfoot()
        {
            // 空中にいる
            if (!isGrounded) return SWalkSound.Surface.None;
            // 止まっている
            if (controller.velocity.sqrMagnitude < 0.01f) return SWalkSound.Surface.None;

            Vector3 playerPosition = controller.transform.position;

            // 優先度の高い順に調べていく
            foreach (var surface in WalkSoundPriority)
            {
                if (IsInsideOfArea(surface, playerPosition))
                    return surface;
            }

            return SWalkSound.Surface.Default;
        }

        /// <summary>
        /// この地形エリア内に、指定された座標が入っているか
        /// </summary>
        /// <remarks>計算コスト高めなので、多用しないこと</remarks>
        private bool IsInsideOfArea(SWalkSound.Surface surface, Vector3 position)
        {
            return IsInsideOfAnyBorder(
                walkSoundBorders[surface],
                position,
                BorderLayer.WalkSound.Get(surface)
            );
        }

        //入力方向と体のなす角が走行時の視野角に収まっているかの正誤、入力からワールドに合わせた移動方向のデータを受け取る
        private bool IsMovingForward(Vector3 inputVec)
        {
            Vector2 moveDirectionXZ = new Vector2(inputVec.x, inputVec.z).normalized;
            Vector2 playerForwardXZ = new Vector2(transform.forward.x, transform.forward.z).normalized;
            float threshold = Mathf.Cos(param.SprintDirectionAllowedAngle * 0.5f * Mathf.Deg2Rad);
            // "前方"に向かって移動している必要がある,normalise input direction again
            return (Vector2.Dot(playerForwardXZ, moveDirectionXZ) > threshold);
        }

        /// <summary>
        /// プレイヤーの少し前方(足元)から下方向にレイを飛ばし、<br/>
        /// 何m下部で地面にヒットしたかを検出する<br/>
        /// </summary>
        /// <param name="forwardDistance">プレイヤーの足元の座標より、前方に 〇[m] 離れたところからレイを飛ばす</param>
        /// <param name="height">成功した場合、レイを飛ばした位置とヒットした地面のY座標の差の絶対値(正)<br/>
        /// 失敗した場合、0.0f</param>
        /// <returns>成功したら true、失敗したら false</returns>
        private bool TryMeasureCliffHeight(float forwardDistance, out float height)
        {
            // 十分長く取る
            const float RayLength = 1000.0f;

            if (Physics.Raycast(
                    transform.position + transform.forward * forwardDistance,
                    Vector3.down,
                    out RaycastHit hitInfo,
                    RayLength,
                    param.GroundLayers,
                    QueryTriggerInteraction.Ignore
                ))
            {
                height = hitInfo.distance;
                return true;
            }
            else
            {
                height = 0.0f;
                return false;
            }
        }

        private static bool IsInsideOfAnyBorder(IReadOnlyList<Border> borders, Vector3 position, byte targetLayer)
        {
            for (int i = 0; i < borders.Count; i++)
            {
                Border border = borders[i];
                if (border.DoesContain(position, targetLayer))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}