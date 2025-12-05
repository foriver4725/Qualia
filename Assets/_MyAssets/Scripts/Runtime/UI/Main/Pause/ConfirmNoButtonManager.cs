using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ConfirmNoButtonManager : AButtonManager
    {
        // ポーズUIで最初に選択するため
        [SerializeField] private ASelectableButtonManager resumeButtonManager;

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

            resumeButtonManager.SelectThisForciblyUnsafe();
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.OnPauseConfirm, false);
        }
    }
}
