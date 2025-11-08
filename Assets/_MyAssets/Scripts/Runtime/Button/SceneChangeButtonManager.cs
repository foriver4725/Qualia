using MyScripts.Common.Button;

namespace MyScripts.Runtime
{
    internal sealed class SceneChangeButtonManager : ATextButtonManager
    {
        [SerializeField] private Scene targetScene;
        [SerializeField] private SSoundSetting soundSetting;
        [SerializeField] private ButtonSoundPlayer soundPlayer;

        private bool isClickEnabled = true;

        // どれか一つが押されたら、他のボタンは無効になる
        // 再度有効になることはない
        private static SceneChangeButtonManager[] linkedButtons = null;

        private protected sealed override void OnJustAfterAwake()
        {
            if (linkedButtons == null)
                linkedButtons = FindObjectsByType<SceneChangeButtonManager>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        private void OnDestroy()
        {
            if (linkedButtons != null)
                linkedButtons = null;
        }

        private protected sealed override void OnClickSucceeded()
        {
            SetLinkedButtonsClicked();
            LoadManager.Instance.BeginLoad(targetScene);
        }

        private protected sealed override void PlayHoverSe()
        {
            if (soundSetting.DoesPlayButtonHoverSe)
                soundPlayer.LetPlay(SButtonSound.Action.Hover);
        }

        private protected sealed override void PlayClickSe()
        {
            soundPlayer.LetPlay(SButtonSound.Action.Click);
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
