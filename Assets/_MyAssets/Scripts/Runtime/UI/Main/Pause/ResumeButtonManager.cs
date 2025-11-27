namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ResumeButtonManager : AButtonManager
    {
        [SerializeField] private PauseInvoker pauseInvoker;

        // ナンとなく、このコンポーネントでフラグをリセットする
        private protected sealed override void OnJustBeforeAwake()
        {
            GuardFlag.IsLocked = false;
        }

        private protected sealed override void OnClickSucceeded()
            => _ = pauseInvoker.TryUnpause();
    }
}
