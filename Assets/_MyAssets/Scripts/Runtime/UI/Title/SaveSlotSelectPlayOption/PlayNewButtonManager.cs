namespace MyScripts.Runtime.UI.Title
{
    internal sealed class PlayNewButtonManager : Button.ASelectableButtonWithFrameManager
    {
        private protected sealed override bool IsFrontUI => StateManager.Instance.State == State.SaveSlot_Select_PlayOption;

        // 消えているボタンは選択できない
        private protected sealed override bool CanSelect(Button.ASelectableButtonManager button)
            => base.CanSelect(button) && button.Parent.gameObject.activeSelf == true;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            PlayOptions.IsNewGame = true;
            StateManager.Instance.ChangeState(State.SaveSlot_FinalConfirm);
        }
    }
}
