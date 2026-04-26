namespace MyScripts.Runtime.UI.Main
{
    internal sealed class ResumeButtonManager : Button.ASelectableButtonWithFrameManager
    {
        [SerializeField] private PauseInvoker pauseInvoker;
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

            // インゲームの入力と干渉しないように、1フレームだけ無効化しておく.
            InputManager.PlayerControl.MakeJumpInputDisabledUntilNextFrame();
            InputManager.PlayerControl.MakeSprintInputDisabledUntilNextFrame();
            InputManager.InGame.MakeSubmitInputDisabledUntilNextFrame();
            InputManager.InGame.MakeCancelInputDisabledUntilNextFrame();
            InputManager.InGame.MakeEscapeInputDisabledUntilNextFrame();

            _ = pauseInvoker.TryUnpause();
        }
    }
}