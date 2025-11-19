using MyScripts.Common.Button;

namespace MyScripts.Runtime
{
    internal sealed class GameQuitButtonManager : ATextButtonManager
    {
        [SerializeField] private Canvas confirmUI;

        private protected sealed override void OnJustBeforeAwake()
        {
            confirmUI.gameObject.SetActive(false);
        }

        private protected sealed override void OnClickSucceeded()
        {
            confirmUI.gameObject.SetActive(true);
        }
    }
}
