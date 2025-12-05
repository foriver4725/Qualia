namespace MyScripts.Runtime.UI.Title.SaveSlot
{
    internal sealed class BackButtonManager : Button.AButtonManager
    {
        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.SelectSaveSlot && InputManager.OutGame.Cancel)
            {
                InputManager.OutGame.MakeCancelInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
            => UIActivationManager.Instance.SetActive(UIActivationManager.UI.SelectSaveSlot, false);
    }
}
