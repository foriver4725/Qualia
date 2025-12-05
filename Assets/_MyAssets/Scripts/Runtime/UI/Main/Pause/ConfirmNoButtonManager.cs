using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ConfirmNoButtonManager : Button.AButtonManager
    {
        // 最初に選択されるもの
        [SerializeField] private ResumeButtonManager resumeButtonManager;

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

            SelectFrameManager.Instance.Reselect(resumeButtonManager);
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.OnPauseConfirm, false);
        }
    }
}
