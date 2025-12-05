using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title
{
    internal sealed class StartButtonManager : AButtonManager
    {
        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.None && InputManager.OutGame.Submit)
            {
                InputManager.OutGame.MakeSubmitInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            UIActivationManager.Instance.SetActive(UIActivationManager.UI.SaveSlot, true);
        }
    }
}
