namespace MyScripts.Runtime.UI.Title.SelectSaveSlotUI
{
    internal sealed class SubmitButtonManager : Button.AButtonManager
    {
        [SerializeField] private Manager manager;

        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.SelectSaveSlot && InputManager.OutGame.Submit)
            {
                InputManager.OutGame.MakeSubmitInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
            => manager.Submit();
    }
}
