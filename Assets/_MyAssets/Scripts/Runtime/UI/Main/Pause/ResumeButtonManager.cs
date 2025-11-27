namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ResumeButtonManager : AButtonManager
    {
        [SerializeField] private PauseInvoker pauseInvoker;

        private protected sealed override void OnClickSucceeded()
            => _ = pauseInvoker.TryUnpause();
    }
}
