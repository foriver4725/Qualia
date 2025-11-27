namespace MyScripts.Runtime.UI.Title.SelectSaveSlotUI
{
    internal sealed class SubmitConfirmNoButtonManager : AButtonManager
    {
        [SerializeField] private Canvas optionConfirmCanvas;

        private protected sealed override void OnClickSucceeded()
            => optionConfirmCanvas.gameObject.SetActive(false);
    }
}
