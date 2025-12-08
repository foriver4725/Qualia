using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ResumeButtonManager : ASelectableButtonWithFrameManager
    {
        [SerializeField] private PauseInvoker pauseInvoker;

        private protected sealed override float HoveredScaleCoef => 1.5f;

        private protected sealed override bool IsFrontUI => UIActivationManager.Instance.Front == UIActivationManager.UI.Pause;

        private protected sealed override void OnSubmittedWithSelection()
        {
            base.PlayClickSe();
            this.OnClickSucceeded();
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            _ = pauseInvoker.TryUnpause();
        }
    }
}
