namespace MyScripts.Runtime.UI.Main.Pause
{
    internal sealed class ConfirmNoButtonManager : AButtonManager
    {
        [SerializeField] private Canvas onPauseConfirmUi;

        private protected sealed override void OnClickSucceeded()
            => onPauseConfirmUi.gameObject.SetActive(false);
    }
}
