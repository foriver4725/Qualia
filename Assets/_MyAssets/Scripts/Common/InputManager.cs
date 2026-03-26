using UnityEngine.InputSystem;

namespace MyScripts.Common
{
    /// <summary>
    /// Enabled フラグをリセットする兼ね合いで、Awake()では入力を参照しない方が好ましい
    /// </summary>
    internal static partial class InputManager
    {
        internal enum Device : byte
        {
            Unknown = 0,
            KeyboardAndMouse = 1,
            Gamepad = 2,
        }

        // 現在使っているデバイスを返す
        // 判定は単純で、最後に接続されたデバイスを返す
        internal static Device GetCurrentDevice() => InputSystem.devices switch
        {
            [] => Device.Unknown,
            _ => InputSystem.devices[^1] switch
            {
                Keyboard or Mouse => Device.KeyboardAndMouse,
                Gamepad           => Device.Gamepad,
                _                 => Device.Unknown,
            }
        };

        /// <summary>
        /// Input Actions で管理されている入力によらず現在の入力値を参照して、<br/>
        /// 何らかのボタン系統入力が押された瞬間であるかを判定する.<br/>
        /// それっぽくボタン系統に入りそうな入力を順に調べていく(ヒューリスティックなロジック).<br/>
        /// </summary>
        internal static bool CheckForAnyRawInputWasPressedThisFrame()
        {
            Device currentDevice = GetCurrentDevice();

            // 入力デバイスがない場合は、入力があったとみなさない
            if (currentDevice == Device.Unknown)
            {
                return false;
            }

            if (currentDevice == Device.KeyboardAndMouse)
            {
                Keyboard currentKeyboard = Keyboard.current;
                if (currentKeyboard == null) return false;
                // キーの精査はせず、これで簡易に判定
                if (currentKeyboard.anyKey.wasPressedThisFrame) return true;

                Mouse currentMouse = Mouse.current;
                if (currentMouse == null) return false;
                // 左・右・中ボタン
                if (currentMouse.leftButton.wasPressedThisFrame) return true;
                if (currentMouse.rightButton.wasPressedThisFrame) return true;
                if (currentMouse.middleButton.wasPressedThisFrame) return true;

                return false;
            }

            if (currentDevice == Device.Gamepad)
            {
                Gamepad currentGamepad = Gamepad.current;
                if (currentGamepad == null) return false;
                // 基本の4つボタン
                if (currentGamepad.buttonNorth.wasPressedThisFrame) return true;
                if (currentGamepad.buttonWest.wasPressedThisFrame) return true;
                if (currentGamepad.buttonSouth.wasPressedThisFrame) return true;
                if (currentGamepad.buttonEast.wasPressedThisFrame) return true;
                // D-Pad
                if (currentGamepad.dpad.up.wasPressedThisFrame) return true;
                if (currentGamepad.dpad.left.wasPressedThisFrame) return true;
                if (currentGamepad.dpad.down.wasPressedThisFrame) return true;
                if (currentGamepad.dpad.right.wasPressedThisFrame) return true;
                // スティック押し込み
                if (currentGamepad.leftStickButton.wasPressedThisFrame) return true;
                if (currentGamepad.rightStickButton.wasPressedThisFrame) return true;
                // ショルダー・トリガー
                if (currentGamepad.leftShoulder.wasPressedThisFrame) return true;
                if (currentGamepad.rightShoulder.wasPressedThisFrame) return true;
                if (currentGamepad.leftTrigger.wasPressedThisFrame) return true;
                if (currentGamepad.rightTrigger.wasPressedThisFrame) return true;

                return false;
            }

            return false;
        }

        internal static void EnableAllInputs()
        {
            PlayerControl.Enabled = true;
            InGame.Enabled = true;
            InGame.EscapeEnabled = true;
            OutGame.Enabled = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Enabled = true;
#endif
        }

        internal static void DisableAllInputs()
        {
            PlayerControl.Enabled = false;
            InGame.Enabled = false;
            InGame.EscapeEnabled = false;
            OutGame.Enabled = false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Enabled = false;
#endif
        }

        private sealed class MakeClickInputDisabledUntilNextFrameInfo
        {
            internal bool IsEnabled { get; private set; } = true;

            private bool isCurrentlyDisabledUntilNextFrame = false;

            internal void Invoke(PlayerLoopTiming flagResetTiming = PlayerLoopTiming.EarlyUpdate)
            {
                ImplAsync(this, flagResetTiming).Forget();

                static async UniTaskVoid ImplAsync(
                    MakeClickInputDisabledUntilNextFrameInfo info,
                    PlayerLoopTiming flagResetTiming
                )
                {
                    if (info.isCurrentlyDisabledUntilNextFrame) return;

                    info.isCurrentlyDisabledUntilNextFrame = true;
                    info.IsEnabled = false;
                    await UniTask.Yield(flagResetTiming);
                    info.IsEnabled = true;
                    info.isCurrentlyDisabledUntilNextFrame = false;
                }
            }
        }

        internal static class PlayerControl
        {
            internal static bool Enabled { get; set; } = true;

            private static readonly MakeClickInputDisabledUntilNextFrameInfo jumpDisableUntilNextFrameInfo = new();
            private static readonly MakeClickInputDisabledUntilNextFrameInfo sprintDisableUntilNextFrameInfo = new();

            internal static void MakeJumpInputDisabledUntilNextFrame() => jumpDisableUntilNextFrameInfo.Invoke();
            internal static void MakeSprintInputDisabledUntilNextFrame() => sprintDisableUntilNextFrameInfo.Invoke();

            private static InputInfo move;
            private static InputInfo look;
            private static InputInfo jump;
            private static InputInfo sprint;

            internal static Vector2 Move => (Enabled) ? move.Vector2 : Vector2.zero;
            internal static Vector2 Look => (Enabled) ? look.Vector2 : Vector2.zero;
            internal static bool Jump => (Enabled && jumpDisableUntilNextFrameInfo.IsEnabled) ? jump.Bool : false;
            internal static bool Sprint => (Enabled && sprintDisableUntilNextFrameInfo.IsEnabled) ? sprint.Bool : false;

            internal static void Bind(MyActions.PlayerControlActions actions)
            {
                move = Create(actions.Move, InputType.Value2);
                look = Create(actions.Look, InputType.Value2);
                jump = Create(actions.Jump, InputType.Click);
                sprint = Create(actions.Sprint, InputType.Value0);
            }
        }

        internal static class InGame
        {
            internal static bool Enabled { get; set; } = true;

            // Escape は特殊な入力のため、個別に管理する (全体の Enabled に依存しない)
            internal static bool EscapeEnabled { get; set; } = true;

            private static readonly MakeClickInputDisabledUntilNextFrameInfo submitDisableUntilNextFrameInfo = new();
            private static readonly MakeClickInputDisabledUntilNextFrameInfo cancelDisableUntilNextFrameInfo = new();
            private static readonly MakeClickInputDisabledUntilNextFrameInfo escapeDisableUntilNextFrameInfo = new();

            internal static void MakeSubmitInputDisabledUntilNextFrame() => submitDisableUntilNextFrameInfo.Invoke();
            internal static void MakeCancelInputDisabledUntilNextFrame() => cancelDisableUntilNextFrameInfo.Invoke();
            internal static void MakeEscapeInputDisabledUntilNextFrame() => escapeDisableUntilNextFrameInfo.Invoke();

            private static InputInfo submit;
            private static InputInfo cancel;
            private static InputInfo escape;

            internal static bool Submit => (Enabled && submitDisableUntilNextFrameInfo.IsEnabled) ? submit.Bool : false;
            internal static bool Cancel => (Enabled && cancelDisableUntilNextFrameInfo.IsEnabled) ? cancel.Bool : false;

            // InGame <-> OutGame の橋渡しをするので、Enabled とは独立している
            internal static bool Escape =>
                (EscapeEnabled && escapeDisableUntilNextFrameInfo.IsEnabled) ? escape.Bool : false;

            internal static void Bind(MyActions.InGameActions actions)
            {
                submit = Create(actions.Submit, InputType.Click);
                cancel = Create(actions.Cancel, InputType.Click);
                escape = Create(actions.Escape, InputType.Click);
            }
        }

        internal static class OutGame
        {
            internal static bool Enabled { get; set; } = true;

            private static readonly MakeClickInputDisabledUntilNextFrameInfo submitDisableUntilNextFrameInfo = new();
            private static readonly MakeClickInputDisabledUntilNextFrameInfo cancelDisableUntilNextFrameInfo = new();
            private static readonly MakeClickInputDisabledUntilNextFrameInfo moveLeftDisableUntilNextFrameInfo = new();
            private static readonly MakeClickInputDisabledUntilNextFrameInfo moveRightDisableUntilNextFrameInfo = new();
            private static readonly MakeClickInputDisabledUntilNextFrameInfo moveDownDisableUntilNextFrameInfo = new();
            private static readonly MakeClickInputDisabledUntilNextFrameInfo moveUpDisableUntilNextFrameInfo = new();

            internal static void MakeSubmitInputDisabledUntilNextFrame() => submitDisableUntilNextFrameInfo.Invoke();
            internal static void MakeCancelInputDisabledUntilNextFrame() => cancelDisableUntilNextFrameInfo.Invoke();

            internal static void MakeMoveLeftInputDisabledUntilNextFrame() =>
                moveLeftDisableUntilNextFrameInfo.Invoke();

            internal static void MakeMoveRightInputDisabledUntilNextFrame() =>
                moveRightDisableUntilNextFrameInfo.Invoke();

            internal static void MakeMoveDownInputDisabledUntilNextFrame() =>
                moveDownDisableUntilNextFrameInfo.Invoke();

            internal static void MakeMoveUpInputDisabledUntilNextFrame() => moveUpDisableUntilNextFrameInfo.Invoke();

            private static InputInfo submit;
            private static InputInfo cancel;
            private static InputInfo moveLeft;
            private static InputInfo moveRight;
            private static InputInfo moveDown;
            private static InputInfo moveUp;

            internal static bool Submit => (Enabled && submitDisableUntilNextFrameInfo.IsEnabled) ? submit.Bool : false;
            internal static bool Cancel => (Enabled && cancelDisableUntilNextFrameInfo.IsEnabled) ? cancel.Bool : false;

            internal static bool MoveLeft =>
                (Enabled && moveLeftDisableUntilNextFrameInfo.IsEnabled) ? moveLeft.Bool : false;

            internal static bool MoveRight =>
                (Enabled && moveRightDisableUntilNextFrameInfo.IsEnabled) ? moveRight.Bool : false;

            internal static bool MoveDown =>
                (Enabled && moveDownDisableUntilNextFrameInfo.IsEnabled) ? moveDown.Bool : false;

            internal static bool MoveUp => (Enabled && moveUpDisableUntilNextFrameInfo.IsEnabled) ? moveUp.Bool : false;

            internal static void Bind(MyActions.OutGameActions actions)
            {
                submit = Create(actions.Submit, InputType.Click);
                cancel = Create(actions.Cancel, InputType.Click);
                moveLeft = Create(actions.MoveLeft, InputType.Click);
                moveRight = Create(actions.MoveRight, InputType.Click);
                moveDown = Create(actions.MoveDown, InputType.Click);
                moveUp = Create(actions.MoveUp, InputType.Click);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal static class Debug
        {
            internal static bool Enabled { get; set; } = true;

            private static readonly MakeClickInputDisabledUntilNextFrameInfo fastenMoveSpeedDisableUntilNextFrameInfo =
                new();

            private static readonly MakeClickInputDisabledUntilNextFrameInfo
                setGraphicQualityLowDisableUntilNextFrameInfo = new();

            private static readonly MakeClickInputDisabledUntilNextFrameInfo
                setGraphicQualityMediumDisableUntilNextFrameInfo = new();

            private static readonly MakeClickInputDisabledUntilNextFrameInfo
                setGraphicQualityHighDisableUntilNextFrameInfo = new();

            internal static void MakeFastenMoveSpeedInputDisabledUntilNextFrame() =>
                fastenMoveSpeedDisableUntilNextFrameInfo.Invoke();

            internal static void MakeSetGraphicQualityLowInputDisabledUntilNextFrame() =>
                setGraphicQualityLowDisableUntilNextFrameInfo.Invoke();

            internal static void MakeSetGraphicQualityMediumInputDisabledUntilNextFrame() =>
                setGraphicQualityMediumDisableUntilNextFrameInfo.Invoke();

            internal static void MakeSetGraphicQualityHighInputDisabledUntilNextFrame() =>
                setGraphicQualityHighDisableUntilNextFrameInfo.Invoke();

            private static InputInfo fastenMoveSpeed;
            private static InputInfo setGraphicQualityLow;
            private static InputInfo setGraphicQualityMedium;
            private static InputInfo setGraphicQualityHigh;

            internal static bool FastenMoveSpeed => (Enabled && fastenMoveSpeedDisableUntilNextFrameInfo.IsEnabled)
                ? fastenMoveSpeed.Bool
                : false;

            internal static bool SetGraphicQualityLow =>
                (Enabled && setGraphicQualityLowDisableUntilNextFrameInfo.IsEnabled)
                    ? setGraphicQualityLow.Bool
                    : false;

            internal static bool SetGraphicQualityMedium =>
                (Enabled && setGraphicQualityMediumDisableUntilNextFrameInfo.IsEnabled)
                    ? setGraphicQualityMedium.Bool
                    : false;

            internal static bool SetGraphicQualityHigh =>
                (Enabled && setGraphicQualityHighDisableUntilNextFrameInfo.IsEnabled)
                    ? setGraphicQualityHigh.Bool
                    : false;

            internal static void Bind(MyActions.DebugActions actions)
            {
                fastenMoveSpeed = Create(actions.FastenMoveSpeed, InputType.Value0);
                setGraphicQualityLow = Create(actions.SetGraphicQualityLow, InputType.Click);
                setGraphicQualityMedium = Create(actions.SetGraphicQualityMedium, InputType.Click);
                setGraphicQualityHigh = Create(actions.SetGraphicQualityHigh, InputType.Click);
            }
        }
#endif

        private static void Bind()
        {
            PlayerControl.Bind(source.PlayerControl);
            InGame.Bind(source.InGame);
            OutGame.Bind(source.OutGame);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Bind(source.Debug);
#endif
        }
    }
}