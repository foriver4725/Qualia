namespace MyScripts.Runtime.UI.Main
{
    /// <summary>
    /// 確認UIの決定ボタンなので、ある種マネージャーみたいな役割も担う
    /// </summary>
    internal sealed class BackConfirmYesButtonManager : Button.ASelectableButtonWithFrameManager
    {
        private protected sealed override bool IsFrontUI => StateManager.Instance.State == State.Back_Confirm;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            if (BackOptions.ChosenBackType == BackType.BackToTitle)
            {
                if (!LoadManager.Instance.HasBegun)
                    LoadManager.Instance.BeginLoad(Scene.Title);
            }
            else // BackType.BackToDesktop
            {
                if (!GameQuitter.HasInvoked)
                    GameQuitter.InvokeQuit();
            }
        }
    }
}
