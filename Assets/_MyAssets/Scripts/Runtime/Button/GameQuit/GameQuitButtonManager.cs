namespace MyScripts.Runtime
{
    internal sealed class GameQuitButtonManager : AButtonManager
    {
        [SerializeField] private Canvas confirmUi;

        private protected sealed override void OnJustBeforeAwake()
        {
            confirmUi.gameObject.SetActive(false);
        }

        private protected sealed override void OnClickSucceeded()
        {
            confirmUi.gameObject.SetActive(true);
        }
    }
}
