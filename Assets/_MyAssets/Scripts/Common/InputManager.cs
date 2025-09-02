namespace MyScripts.Common
{
    internal static partial class InputManager
    {
        // PlayerControl
        internal static InputInfo PcMove { get; private set; }
        internal static InputInfo PcLook { get; private set; }
        internal static InputInfo PcJump { get; private set; }
        internal static InputInfo PcSprint { get; private set; }

        // InGame
        internal static InputInfo InGameSubmit { get; private set; }
        internal static InputInfo InGameCancel { get; private set; }
        internal static InputInfo InGameTriggerCharacter { get; private set; }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Debug
        internal static InputInfo DebugFastenTimeLimit { get; private set; }
        internal static InputInfo DebugFastenMoveSpeed { get; private set; }
#endif

        private static void Bind()
        {
            PcMove = Create(source.PlayerControl.Move, InputType.Value2);
            PcLook = Create(source.PlayerControl.Look, InputType.Value2);
            PcJump = Create(source.PlayerControl.Jump, InputType.Click);
            PcSprint = Create(source.PlayerControl.Sprint, InputType.Value0);

            InGameSubmit = Create(source.InGame.Submit, InputType.Click);
            InGameCancel = Create(source.InGame.Cancel, InputType.Click);
            InGameTriggerCharacter = Create(source.InGame.TriggerCharacter, InputType.Click);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            DebugFastenTimeLimit = Create(source.Debug.FastenTimeLimit, InputType.Click);
            DebugFastenMoveSpeed = Create(source.Debug.FastenMoveSpeed, InputType.Value0);
#endif
        }
    }
}
