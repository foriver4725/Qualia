namespace MyScripts.Runtime.OutGame.Title.SelectSaveSlotUI
{
    internal sealed class SubmitButtonManager : AButtonManager
    {
        [SerializeField] private Manager manager;

        private protected sealed override void OnClickSucceeded()
            => manager.Submit();
    }
}
