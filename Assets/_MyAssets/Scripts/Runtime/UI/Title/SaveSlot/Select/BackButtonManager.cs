using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Select
{
    internal sealed class BackButtonManager : AButtonManager
    {
        private void Update()
        {
            if (UIActivationManager.Instance.Front == UIActivationManager.UI.SaveSlot
                && StateRootObjectManager.Instance.State == State.Select
                && InputManager.OutGame.Cancel)
            {
                InputManager.OutGame.MakeCancelInputDisabledUntilNextFrame();
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
        {
            base.OnClickSucceeded();

            StateRootObjectManager.Instance.ChangeState(State.None);
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.SaveSlot, false);
        }
    }
}
