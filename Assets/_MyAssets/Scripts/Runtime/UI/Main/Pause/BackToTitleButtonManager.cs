namespace MyScripts.Runtime.UI.Main
{
    internal sealed class BackToTitleButtonManager : Button.ASelectableButtonWithFrameManager
    {
        [SerializeField] private BackConfirmYesButtonManager confirmYesButtonManager;
        [SerializeField] private RectTransform parentRow;

        private protected sealed override float HoveredScaleCoef => 1.5f;
        private protected sealed override Vector2 AnchoredPositionOffset => parentRow.anchoredPosition;

        private protected sealed override bool IsFrontUI => StateManager.Instance.State == State.Pause;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            BackOptions.ChosenBackType = BackType.BackToTitle;
            StateManager.Instance.ChangeState(State.Back_Confirm);
        }
    }
}
