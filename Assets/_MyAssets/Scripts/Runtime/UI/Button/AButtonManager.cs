using MyScripts.Common.Button;

namespace MyScripts.Runtime.UI.Button
{
    internal abstract class AButtonManager : ATextButtonManager
    {
        [SerializeField] private SSoundSetting soundSetting;
        [SerializeField] private ButtonSoundPlayer soundPlayer;

        // ボタンの位置が動的に変わることはない想定なので、シンプルなキャッシュにする
        private bool hasCachedPosition = false;
        private Vector2 position = new(-1, -1);
        internal Vector2 Position
        {
            get
            {
                if (!hasCachedPosition)
                {
                    position = BackgroundImage.rectTransform.anchoredPosition;
                    hasCachedPosition = true;
                }
                return position;
            }
        }

        // ボタンの大きさが動的に変わることはない想定なので、シンプルなキャッシュにする
        private bool hasCachedSize = false;
        private Vector2 size = new(-1, -1);
        internal Vector2 Size
        {
            get
            {
                if (!hasCachedSize)
                {
                    size = BackgroundImage.rectTransform.sizeDelta;
                    hasCachedSize = true;
                }
                return size;
            }
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
    }
}
