namespace MyScripts.Common
{
    internal sealed class InputManager : AInputManager<InputManager>
    {
        // PlayerControl
        internal InputInfo PcMove { get; private set; }
        internal InputInfo PcLook { get; private set; }
        internal InputInfo PcJump { get; private set; }
        internal InputInfo PcSprint { get; private set; }

        // InGame
        internal InputInfo InGameSubmit { get; private set; }
        internal InputInfo InGameCancel { get; private set; }
        internal InputInfo InGameTriggerCharacter { get; private set; }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Debug
        internal InputInfo DebugFastenTimeLimit { get; private set; }
        internal InputInfo DebugFastenMoveSpeed { get; private set; }
#endif

        private protected sealed override void Init()
        {
            PcMove = Setup(Source.PlayerControl.Move, InputType.Value2);
            PcLook = Setup(Source.PlayerControl.Look, InputType.Value2);
            PcJump = Setup(Source.PlayerControl.Jump, InputType.Click);
            PcSprint = Setup(Source.PlayerControl.Sprint, InputType.Value0);

            InGameSubmit = Setup(Source.InGame.Submit, InputType.Click);
            InGameCancel = Setup(Source.InGame.Cancel, InputType.Click);
            InGameTriggerCharacter = Setup(Source.InGame.TriggerCharacter, InputType.Click);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            DebugFastenTimeLimit = Setup(Source.Debug.FastenTimeLimit, InputType.Click);
            DebugFastenMoveSpeed = Setup(Source.Debug.FastenMoveSpeed, InputType.Value0);
#endif
        }
    }
}
