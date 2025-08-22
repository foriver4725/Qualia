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
            PcMove = Setup(_ia.PlayerControl.Move, InputType.Value2);
            PcLook = Setup(_ia.PlayerControl.Look, InputType.Value2);
            PcJump = Setup(_ia.PlayerControl.Jump, InputType.Click);
            PcSprint = Setup(_ia.PlayerControl.Sprint, InputType.Value0);

            InGameSubmit = Setup(_ia.InGame.Submit, InputType.Click);
            InGameCancel = Setup(_ia.InGame.Cancel, InputType.Click);
            InGameTriggerCharacter = Setup(_ia.InGame.TriggerCharacter, InputType.Click);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            DebugFastenTimeLimit = Setup(_ia.Debug.FastenTimeLimit, InputType.Click);
            DebugFastenMoveSpeed = Setup(_ia.Debug.FastenMoveSpeed, InputType.Value0);
#endif
        }
    }
}
