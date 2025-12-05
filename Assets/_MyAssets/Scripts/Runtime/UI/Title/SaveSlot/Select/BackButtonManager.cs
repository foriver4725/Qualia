using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Title.SaveSlot.Select
{
    internal sealed class BackButtonManager : AButtonManager
    {
        private protected sealed override void OnClickSucceeded()
        {
            UIActivationManager.Instance.SetActive(UIActivationManager.UI.SaveSlot, false);
        }
    }
}
