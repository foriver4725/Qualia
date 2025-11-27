namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ResumeButtonManager : ASelectableButtonManager
    {
        [SerializeField] private PauseInvoker pauseInvoker;

        private protected sealed override void OnSubmittedWithSelection()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.Pause)
            {
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
            => _ = pauseInvoker.TryUnpause();
    }
}
