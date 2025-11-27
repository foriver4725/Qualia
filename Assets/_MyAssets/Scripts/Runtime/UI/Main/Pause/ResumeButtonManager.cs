namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ResumeButtonManager : ASelectableButtonManager
    {
        [SerializeField] private PauseInvoker pauseInvoker;

        private protected sealed override void OnSubmittedWithSelection()
        {
            // 結局 Try...() で可能かどうか判定してくれるので、チェックは必要ないと思う
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
            => _ = pauseInvoker.TryUnpause();
    }
}
