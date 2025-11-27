using UnityEngine.InputSystem;
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
				SWalkSound.Surface.Sand => sand,
				SWalkSound.Surface.Rock => rock,
				SWalkSound.Surface.Water => water,
				_ => throw new ArgumentOutOfRangeException(nameof(surface), surface, null)
			};
		}

		[Header("Player Control")]
		[SerializeField] private CharacterController controller;
		[SerializeField] private CameraFOVManager cameraFOVManager;
		[SerializeField] private PlayerControlSoundPlayer soundPlayer;
		[SerializeField] private Transform cinemachineCameraTarget;
		[SerializeField] private Transform teleportBackPoint;
		[Space(10)]
		[Header("Walk Sound")]
		[SerializeField] private WalkSoundPlayer walkSoundPlayer;
		[SerializeField] private WalkSoundBorderRoots walkSoundBorderRoots;

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
		private bool isJumping = false;
		private bool isSprinting = false;
		private bool isDoingInertiaJump = false;
		private bool onInertiaJumpCt = false;

		// timeout deltatime
		// Awake で初期化
		private SPlayerControl param;
		private float jumpTimeoutDelta;
		private float fallTimeoutDelta;

		// input
		private Vector2 MoveInput => IsPcInputEnabled ? InputManager.PcMove.Vector2 : Vector2.zero;
		private Vector2 LookInput => IsPcInputEnabled ? InputManager.PcLook.Vector2 : Vector2.zero;
		private bool JumpInput => IsPcInputEnabled ? InputManager.PcJump.Bool : false;
		private bool SprintInput => IsPcInputEnabled ? InputManager.PcSprint.Bool : false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		private bool DebugFastenMoveSpeedInput => IsPcInputEnabled ? InputManager.DebugFastenMoveSpeed.Bool : false;
#endif

		// constraints
		internal bool IsPcInputEnabled { get; set; } = true;
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
		private static readonly ReadOnlyCollection<SWalkSound.Surface> WalkSoundPriority = Array.AsReadOnly(new SWalkSound.Surface[]
		{
			SWalkSound.Surface.Rock,
			SWalkSound.Surface.Water,
			SWalkSound.Surface.Sand,
			SWalkSound.Surface.Grass,
		});
		private readonly Dictionary<SWalkSound.Surface, ReadOnlyCollection<Border>> walkSoundBorders = new(); // Awake で初期化

		#region Public Methods and Properties

		internal Collider Collider => controller;

		internal void Teleport(Vector3 position, Vector3 forward)
		{
			// 一応、切り替わり中の挙動制限も適用しておく
			IsPcInputEnabled = false;
			IsOwnGravityEnabled = false;
			CanApplyVelocityDelta = false;

			{
				transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));
			}

			CanApplyVelocityDelta = true;
			IsOwnGravityEnabled = true;
			IsPcInputEnabled = true;
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
			AttenuateNativeHorizontalVelocity();
			InputAndFinallyMove();
			TeleportBackWhenInvalidPosition();

			UpdateFOVsSprintMode();
			UpdateWalkSound();

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
			Vector3 spherePosition = new(transform.position.x, transform.position.y + param.GroundCheckOffset, transform.position.z);
			isGrounded = Physics.CheckSphere(spherePosition, param.GroundCheckRadius, param.GroundLayers, QueryTriggerInteraction.Ignore);

			hasBecameGroundedThisFrame = isGrounded && !isGroundedPrev;
			hasBecameNotGroundedThisFrame = !isGrounded && isGroundedPrev;

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
			// 機能が無効なら論外！！
			if (!param.EnableInertiaJump) return;

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

			//? 目の前が崖であるべきか？

			// 処理を行える

			isDoingInertiaJump = true;

			Vector2 directionXZ = new Vector2(realHorizontalVelocity.x, realHorizontalVelocity.z).normalized;
			Vector3 velocity = new(
				directionXZ.x * param.InertiaJumpVelocity.x,
				param.InertiaJumpVelocity.y,
				directionXZ.y * param.InertiaJumpVelocity.z
			);

			ApplyOuterVelocity(velocity);

			soundPlayer.LetPlay(SPlayerControlSound.Action.InertiaJump);
		}

		private void CameraRotation()
		{
			// get input
			Vector2 input = LookInput;

			// if there is an input
			if (input.sqrMagnitude >= 0.01f)
			{
				//Don't multiply mouse input by Time.deltaTime
				bool isCurrentDeviceMouse = Mouse.current != null && Mouse.current.wasUpdatedThisFrame;
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
				float attenuationRate = isGrounded ?
					param.NativeHorizontalVelocityAttenuationRateOnGround : param.NativeHorizontalVelocityAttenuationRateInAir;
				nativeHorizontalVelocity -= nativeHorizontalVelocity * attenuationRate;

				if (nativeHorizontalVelocity.sqrMagnitude < 1e-4f)
				{
					nativeHorizontalVelocity = Vector2.zero;
				}
			}
		}

		private void InputAndFinallyMove()
		{
			// get input
			Vector2 input = MoveInput;
			bool isSprintingInput = SprintInput;
			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			bool hasInput = input != Vector2.zero;
			isSprinting = isSprintingInput && hasInput;

			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = isSprintingInput ? param.MoveSpeed * param.SprintSpeedMultiplier : param.MoveSpeed;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			// for debug, make the player move faster while has the input
			if (DebugFastenMoveSpeedInput)
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
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * param.MoveAcceleration);

				// round speed to 3 decimal places
				speed = Mathf.Round(speed * 1000f) / 1000f;
			}
			else
			{
				speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(input.x, 0.0f, input.y).normalized;

			// if there is a move input rotate player when the player is moving
			if (hasInput)
			{
				// move
				inputDirection = transform.right * input.x + transform.forward * input.y;
			}

			// normalise input direction again
			inputDirection.Normalize();

			// when it is the timing, make move input insensitive
			if (param.MoveInputInsensitiveTiming == SPlayerControl.MoveInputInsensitiveTimingType.WhileInAir)
			{
				if (!isGrounded)
				{
					inputDirection *= param.MoveInputInsensitiveRate;
				}
			}
			else if (param.MoveInputInsensitiveTiming == SPlayerControl.MoveInputInsensitiveTimingType.WhileInAirAndWhenOuterVelocityIsNotZero)
			{
				if (!isGrounded && nativeHorizontalVelocity != Vector2.zero)
				{
					inputDirection *= param.MoveInputInsensitiveRate;
				}
			}

			// calculate the real velocity
			realHorizontalVelocity = inputDirection * speed + new Vector3(nativeHorizontalVelocity.x, 0.0f, nativeHorizontalVelocity.y);
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
				// get input
				bool input = JumpInput;

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
				// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
				if (verticalVelocity < TerminalVelocity)
				{
					verticalVelocity += param.OwnGravity * Time.deltaTime;
				}
			}
		}

		private void TeleportBackWhenInvalidPosition()
		{
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
				if (IsPlayerInsideOfAnyBorder(walkSoundBorders[surface], playerPosition, BorderLayer.WalkSound.Get(surface)))
					return surface;
			}

			return SWalkSound.Surface.Default;
		}

		private static bool IsPlayerInsideOfAnyBorder(IReadOnlyList<Border> borders, Vector3 playerPosition, byte targetLayer)
		{
			for (int i = 0; i < borders.Count; i++)
			{
				Border border = borders[i];
				if (border.DoesContain(playerPosition, targetLayer))
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
