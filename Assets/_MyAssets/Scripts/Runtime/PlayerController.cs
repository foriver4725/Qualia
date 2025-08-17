using UnityEngine.InputSystem;
using MyScripts.SO.Parameter;

namespace MyScripts.Runtime
{
	[RequireComponent(typeof(CharacterController))]
	internal sealed class PlayerController : MonoBehaviour
	{
		[SerializeField] private CharacterController controller;
		[SerializeField] private SPlayerControl param;
		[SerializeField] private Transform cinemachineCameraTarget;

		// cinemachine
		private float cinemachineTargetPitch;

		// player
		private float rotationVelocity;
		private Vector2 nativeHorizontalVelocity = Vector2.zero; // 入力によらない水平移動速度 (段々減衰していき,0になる)
		private float verticalVelocity;
		private static readonly float TerminalVelocity = 53.0f;
		private bool isGrounded = true;

		// timeout deltatime
		// Awake で初期化
		private float jumpTimeoutDelta;
		private float fallTimeoutDelta;

		// input
		private Vector2 MoveInput => IsPcInputEnabled ? InputManager.Instance.PcMove.Vector2 : Vector2.zero;
		private Vector2 LookInput => IsPcInputEnabled ? InputManager.Instance.PcLook.Vector2 : Vector2.zero;
		private bool JumpInput => IsPcInputEnabled ? InputManager.Instance.PcJump.Bool : false;
		private bool SprintInput => IsPcInputEnabled ? InputManager.Instance.PcSprint.Bool : false;

		internal bool IsPcInputEnabled { get; set; } = true;

		private bool isOwnGravityEnabled = true;
		internal bool IsOwnGravityEnabled
		{
			get => isOwnGravityEnabled;
			set
			{
				isOwnGravityEnabled = value;

				// 重力を無効化する時は、鉛直方向の速度もリセットする
				if (!value)
				{
					verticalVelocity = 0f;
				}
			}
		}

		private void Awake()
		{
			// reset our timeouts on start
			jumpTimeoutDelta = param.JumpTimeout;
			fallTimeoutDelta = param.FallTimeout;
		}

		private void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			AttenuateNativeHorizontalVelocity();
			InputAndFinallyMove();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		// 入力・重力によるものではない、外力による速度増加
		private void ApplyOuterVelocity(Vector3 velocity)
		{
			nativeHorizontalVelocity += new Vector2(velocity.x, velocity.z);
			verticalVelocity += velocity.y;
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new(transform.position.x, transform.position.y + param.GroundCheckOffset, transform.position.z);
			isGrounded = Physics.CheckSphere(spherePosition, param.GroundCheckRadius, param.GroundLayers, QueryTriggerInteraction.Ignore);
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
				cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, param.CameraClamps.x, param.CameraClamps.y);

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
			bool isDoingSprint = SprintInput;

			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = isDoingSprint ? param.MoveSpeed * param.SprintSpeedMultiplier : param.MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (input == Vector2.zero) targetSpeed = 0.0f;

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

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (input != Vector2.zero)
			{
				// move
				inputDirection = transform.right * input.x + transform.forward * input.y;
			}

			// calculate the real velocity
			Vector3 realHorizontalVelocity = inputDirection.normalized * speed + new Vector3(nativeHorizontalVelocity.x, 0.0f, nativeHorizontalVelocity.y);
			Vector3 realVelocity = realHorizontalVelocity + new Vector3(0.0f, verticalVelocity, 0.0f);

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
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					verticalVelocity = Mathf.Sqrt(param.JumpHeight * -2f * param.OwnGravity);
				}

				// jump timeout
				if (jumpTimeoutDelta >= 0.0f)
				{
					jumpTimeoutDelta -= Time.deltaTime;
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

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new(1.0f, 0.0f, 0.0f, 0.35f);

			if (isGrounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y + param.GroundCheckOffset, transform.position.z), param.GroundCheckRadius);
		}
	}
}