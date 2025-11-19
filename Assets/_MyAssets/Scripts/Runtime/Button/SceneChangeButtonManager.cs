namespace MyScripts.Runtime
{
    internal class SceneChangeButtonManager : AButtonManager
    {
        [SerializeField] private Scene targetScene;

        private bool isClickEnabled = true;

        // どれか一つが押されたら、他のボタンは無効になる
        // 再度有効になることはない
        private static SceneChangeButtonManager[] linkedButtons = null;

        private protected sealed override void OnJustAfterAwake()
        {
            linkedButtons ??= FindObjectsByType<SceneChangeButtonManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        private void OnDestroy()
        {
            if (linkedButtons != null)
                linkedButtons = null;
        }

        private protected sealed override void OnClickSucceeded()
        {
            if (!isClickEnabled) return;

            SetLinkedButtonsClicked();
            InvokeLoad();
        }

        private protected virtual void InvokeLoad()
        {
            LoadManager.Instance.BeginLoad(targetScene);
        }

        private void SetLinkedButtonsClicked()
        {
            foreach (var linkedButton in linkedButtons)
            {
                linkedButton.isClickEnabled = false;
            }
        }
    }
}
