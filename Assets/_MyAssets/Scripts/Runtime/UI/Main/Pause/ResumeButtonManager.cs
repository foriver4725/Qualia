namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ResumeButtonManager : ASelectableButtonManager
    {
        [SerializeField] private PauseInvoker pauseInvoker;

        private protected sealed override UIActivationManager.UI LocatedUI => UIActivationManager.UI.Pause;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
            => _ = pauseInvoker.TryUnpause();
    }
}
