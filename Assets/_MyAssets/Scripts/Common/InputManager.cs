namespace MyScripts.Common
{
    internal sealed class InputManager : AInputManager<InputManager>
    {
        // InGame
        public InputInfo InGameSubmit { get; private set; }
        public InputInfo InGameCancel { get; private set; }
        public InputInfo InGameTriggerCharacter { get; private set; }

        private protected sealed override void Init()
        {
            InGameSubmit = Setup(_ia.InGame.Submit, InputType.Click);
            InGameCancel = Setup(_ia.InGame.Cancel, InputType.Click);
            InGameTriggerCharacter = Setup(_ia.InGame.TriggerCharacter, InputType.Click);
        }
    }
}
