namespace MyScripts.Runtime.UI.Title.GameQuit
{
    internal sealed class GameQuitButtonManager : AButtonManager
    {
        [SerializeField] private Canvas confirmUi;

        private void Update()
        {
            if (confirmUi.gameObject.activeSelf == false && InputManager.OutGame.Cancel)
            {
                base.PlayClickSe();
                this.OnClickSucceeded();
            }
        }

        private protected sealed override void OnClickSucceeded()
        {
            confirmUi.gameObject.SetActive(true);
        }
    }
}
