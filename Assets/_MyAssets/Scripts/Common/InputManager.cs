namespace MyScripts.Common
{
    internal sealed class InputManager : AInputManager<InputManager>
    {
        // InGame
        internal InputInfo InGameSubmit { get; private set; }
        internal InputInfo InGameCancel { get; private set; }
        internal InputInfo InGameTriggerCharacter { get; private set; }

        // Debug
        internal InputInfo DebugFastenTimeLimit { get; private set; }

        private protected sealed override void Init()
        {
            InGameSubmit = Setup(_ia.InGame.Submit, InputType.Click);
            InGameCancel = Setup(_ia.InGame.Cancel, InputType.Click);
            InGameTriggerCharacter = Setup(_ia.InGame.TriggerCharacter, InputType.Click);

            DebugFastenTimeLimit = Setup(_ia.Debug.FastenTimeLimit, InputType.Click);
        }
    }
}
