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
        // 判定は単純で、列挙済みデバイスを末尾から見て Keyboard/Mouse/Gamepad に該当するものを返す
        internal static Device GetCurrentDevice()
        {
            for (int i = InputSystem.devices.Count - 1; i >= 0; i--)
            {
                InputDevice device = InputSystem.devices[i];

                // 関係ない他のデバイスは無視する
                if (device is Keyboard or Mouse) return Device.KeyboardAndMouse;
                if (device is Gamepad) return Device.Gamepad;
            }

            return Device.Unknown;
        }

        /// <summary>
        /// Input Actions で管理されている入力によらず現在の入力値を参照して、<br/>
        /// 何らかのボタン系統入力が押された瞬間であるかを判定する.<br/>
        /// それっぽくボタン系統に入りそうな入力を順に調べていく(ヒューリスティックなロジック).<br/>
        /// </summary>
        internal static bool CheckForAnyRawInputWasPressedThisFrame()
        {
            foreach (InputDevice device in InputSystem.devices)
            {
                if (device is Keyboard keyboard)
                {
                    // アルファベットキー
                    if (keyboard.aKey.wasPressedThisFrame) return true;
                    if (keyboard.bKey.wasPressedThisFrame) return true;
                    if (keyboard.cKey.wasPressedThisFrame) return true;
                    if (keyboard.dKey.wasPressedThisFrame) return true;
                    if (keyboard.eKey.wasPressedThisFrame) return true;
                    if (keyboard.fKey.wasPressedThisFrame) return true;
                    if (keyboard.gKey.wasPressedThisFrame) return true;
                    if (keyboard.hKey.wasPressedThisFrame) return true;
                    if (keyboard.iKey.wasPressedThisFrame) return true;
                    if (keyboard.jKey.wasPressedThisFrame) return true;
                    if (keyboard.kKey.wasPressedThisFrame) return true;
                    if (keyboard.lKey.wasPressedThisFrame) return true;
                    if (keyboard.mKey.wasPressedThisFrame) return true;
                    if (keyboard.nKey.wasPressedThisFrame) return true;
                    if (keyboard.oKey.wasPressedThisFrame) return true;
                    if (keyboard.pKey.wasPressedThisFrame) return true;
                    if (keyboard.qKey.wasPressedThisFrame) return true;
                    if (keyboard.rKey.wasPressedThisFrame) return true;
                    if (keyboard.sKey.wasPressedThisFrame) return true;
                    if (keyboard.tKey.wasPressedThisFrame) return true;
                    if (keyboard.uKey.wasPressedThisFrame) return true;
                    if (keyboard.vKey.wasPressedThisFrame) return true;
                    if (keyboard.wKey.wasPressedThisFrame) return true;
                    if (keyboard.xKey.wasPressedThisFrame) return true;
                    if (keyboard.yKey.wasPressedThisFrame) return true;
                    if (keyboard.zKey.wasPressedThisFrame) return true;
                    // 数字キー
                    if (keyboard.digit0Key.wasPressedThisFrame) return true;
                    if (keyboard.digit1Key.wasPressedThisFrame) return true;
                    if (keyboard.digit2Key.wasPressedThisFrame) return true;
                    if (keyboard.digit3Key.wasPressedThisFrame) return true;
                    if (keyboard.digit4Key.wasPressedThisFrame) return true;
                    if (keyboard.digit5Key.wasPressedThisFrame) return true;
                    if (keyboard.digit6Key.wasPressedThisFrame) return true;
                    if (keyboard.digit7Key.wasPressedThisFrame) return true;
                    if (keyboard.digit8Key.wasPressedThisFrame) return true;
                    if (keyboard.digit9Key.wasPressedThisFrame) return true;
                    // 矢印キー
                    if (keyboard.upArrowKey.wasPressedThisFrame) return true;
                    if (keyboard.leftArrowKey.wasPressedThisFrame) return true;
                    if (keyboard.downArrowKey.wasPressedThisFrame) return true;
                    if (keyboard.rightArrowKey.wasPressedThisFrame) return true;
                    // その他のキー
                    if (keyboard.spaceKey.wasPressedThisFrame) return true;
                    if (keyboard.enterKey.wasPressedThisFrame) return true;
                    if (keyboard.tabKey.wasPressedThisFrame) return true;
                    if (keyboard.leftShiftKey.wasPressedThisFrame) return true;
                    if (keyboard.rightShiftKey.wasPressedThisFrame) return true;
                    if (keyboard.leftCtrlKey.wasPressedThisFrame) return true;
                    if (keyboard.rightCtrlKey.wasPressedThisFrame) return true;
                    if (keyboard.leftAltKey.wasPressedThisFrame) return true;
                    if (keyboard.rightAltKey.wasPressedThisFrame) return true;
                    if (keyboard.leftMetaKey.wasPressedThisFrame) return true;
                    if (keyboard.rightMetaKey.wasPressedThisFrame) return true;
                }
                else if (device is Mouse mouse)
                {
                    // 左・右・中ボタン
                    if (mouse.leftButton.wasPressedThisFrame) return true;
                    if (mouse.rightButton.wasPressedThisFrame) return true;
                    if (mouse.middleButton.wasPressedThisFrame) return true;
                }
                else if (device is Gamepad gamepad)
                {
                    // 基本の4つボタン
                    if (gamepad.buttonNorth.wasPressedThisFrame) return true;
                    if (gamepad.buttonWest.wasPressedThisFrame) return true;
                    if (gamepad.buttonSouth.wasPressedThisFrame) return true;
                    if (gamepad.buttonEast.wasPressedThisFrame) return true;
                    // D-Pad
                    if (gamepad.dpad.up.wasPressedThisFrame) return true;
                    if (gamepad.dpad.left.wasPressedThisFrame) return true;
                    if (gamepad.dpad.down.wasPressedThisFrame) return true;
                    if (gamepad.dpad.right.wasPressedThisFrame) return true;
                    // スティック押し込み
                    if (gamepad.leftStickButton.wasPressedThisFrame) return true;
                    if (gamepad.rightStickButton.wasPressedThisFrame) return true;
                    // ショルダー・トリガー
                    if (gamepad.leftShoulder.wasPressedThisFrame) return true;
                    if (gamepad.rightShoulder.wasPressedThisFrame) return true;
                    if (gamepad.leftTrigger.wasPressedThisFrame) return true;
                    if (gamepad.rightTrigger.wasPressedThisFrame) return true;
                }
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