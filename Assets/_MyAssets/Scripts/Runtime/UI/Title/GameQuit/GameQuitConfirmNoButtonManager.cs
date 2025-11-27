namespace MyScripts.Runtime.UI.Title.GameQuit
{
    internal sealed class GameQuitConfirmNoButtonManager : AButtonManager
    {
        [SerializeField] private Canvas confirmUi;

        private protected sealed override void OnClickSucceeded()
        {
            confirmUi.gameObject.SetActive(false);
        }
    }
}
