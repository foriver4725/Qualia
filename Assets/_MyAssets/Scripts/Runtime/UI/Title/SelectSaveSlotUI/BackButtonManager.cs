namespace MyScripts.Runtime.UI.Title.SelectSaveSlotUI
{
    internal sealed class BackButtonManager : AButtonManager
    {
        private protected sealed override void OnClickSucceeded()
            => UIActivationManager.Instance.SetActive(UIActivationManager.UI.SelectSaveSlot, false);
    }
}
