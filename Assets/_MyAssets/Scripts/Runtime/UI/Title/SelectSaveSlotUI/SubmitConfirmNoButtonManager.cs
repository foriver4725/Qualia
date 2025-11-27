namespace MyScripts.Runtime.UI.Title.SelectSaveSlotUI
{
    internal sealed class SubmitConfirmNoButtonManager : AButtonManager
    {
        private protected sealed override void OnClickSucceeded()
            => UIActivationManager.Instance.SetActive(UIActivationManager.UI.OptionConfirm, false);
    }
}
