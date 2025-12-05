using MyScripts.Runtime.UI.Title.SaveSlot;

namespace MyScripts.Runtime.UI.Title
{
    internal sealed class StartButtonManager : Button.AButtonManager
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

            StateRootObjectManager.Instance.ChangeState(State.Select);
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.SaveSlot, true);
        }
    }
}
