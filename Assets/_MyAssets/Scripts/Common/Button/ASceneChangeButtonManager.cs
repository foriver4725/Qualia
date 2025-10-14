namespace MyScripts.Common.Button
{
    internal abstract class ASceneChangeButtonManager : ATextButtonManager
    {
        [SerializeField, Tooltip("どれか一つが押されたら、他のボタンは無効になる")]
        private ASceneChangeButtonManager[] linkedButtons;

        private bool isClickEnabled = true;

        private protected abstract Scene TargetScene { get; }

        private protected sealed override void OnClickSucceeded()
        {
            SetLinkedButtonsClicked();
            LoadManager.Instance.BeginLoad(TargetScene);
        }

        private void SetLinkedButtonsClicked()
        {
            if (!isClickEnabled) return;
            if (linkedButtons == null) return;

            foreach (var linkedButton in linkedButtons)
            {
                if (linkedButton == null) continue;
                linkedButton.isClickEnabled = false;
            }
        }
    }
}
