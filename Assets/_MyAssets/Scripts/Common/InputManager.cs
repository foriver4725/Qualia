namespace MyScripts.Common
{
    /// <summary>
    /// Enabled フラグをリセットする兼ね合いで、Awake()では入力を参照しない方が好ましい
    /// </summary>
    internal static partial class InputManager
    {
        internal static class PlayerControl
        {
            internal static bool Enabled { get; set; } = true;

            private static InputInfo move;
            private static InputInfo look;
            private static InputInfo jump;
            private static InputInfo sprint;

            internal static Vector2 Move => Enabled ? move.Vector2 : Vector2.zero;
            internal static Vector2 Look => Enabled ? look.Vector2 : Vector2.zero;
            internal static bool Jump => Enabled ? jump.Bool : false;
            internal static bool Sprint => Enabled ? sprint.Bool : false;

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

            private static InputInfo submit;
            private static InputInfo cancel;
            private static InputInfo escape;

            internal static bool Submit => Enabled ? submit.Bool : false;
            internal static bool Cancel => Enabled ? cancel.Bool : false;

            // InGame <-> OutGame の橋渡しをするので、常に有効な入力値とする
            internal static bool Escape => escape.Bool; /*Enabled ? escape.Bool : false;*/

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

            private static InputInfo submit;
            private static InputInfo cancel;
            private static InputInfo moveH;
            private static InputInfo moveV;

            internal static bool Submit => Enabled ? submit.Bool : false;
            internal static bool Cancel => Enabled ? cancel.Bool : false;
            internal static int MoveH => Enabled ? (moveH.Float > 0 ? 1 : (moveH.Float < 0 ? -1 : 0)) : 0;
            internal static int MoveV => Enabled ? (moveV.Float > 0 ? 1 : (moveV.Float < 0 ? -1 : 0)) : 0;

            internal static void Bind(MyActions.OutGameActions actions)
            {
                submit = Create(actions.Submit, InputType.Click);
                cancel = Create(actions.Cancel, InputType.Click);
                moveH = Create(actions.MoveH, InputType.Value1);
                moveV = Create(actions.MoveV, InputType.Value1);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal static class Debug
        {
            internal static bool Enabled { get; set; } = true;

            private static InputInfo fastenMoveSpeed;
            private static InputInfo setGraphicQualityLow;
            private static InputInfo setGraphicQualityMedium;
            private static InputInfo setGraphicQualityHigh;

            internal static bool FastenMoveSpeed => Enabled ? fastenMoveSpeed.Bool : false;
            internal static bool SetGraphicQualityLow => Enabled ? setGraphicQualityLow.Bool : false;
            internal static bool SetGraphicQualityMedium => Enabled ? setGraphicQualityMedium.Bool : false;
            internal static bool SetGraphicQualityHigh => Enabled ? setGraphicQualityHigh.Bool : false;

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
