using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ConfirmNoButtonManager : AButtonManager
    {
        [SerializeField] private AViewConstructor pauseViewConstructor;

        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.OnPauseConfirm && InputManager.OutGame.Cancel)
            {
                InputManager.OutGame.MakeCancelInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            pauseViewConstructor.Construct();
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.OnPauseConfirm, false);
        }
    }
}
