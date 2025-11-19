using MyScripts.Common.Button;

namespace MyScripts.Runtime
{
    internal sealed class GameQuitConfirmNoButtonManager : ATextButtonManager
    {
        [SerializeField] private Canvas confirmUI;

        private protected sealed override void OnClickSucceeded()
        {
            confirmUI.gameObject.SetActive(false);
        }
    }
}
