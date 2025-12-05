using MyScripts.Common.Button;

namespace MyScripts.Runtime.UI.Button
{
    internal abstract class AButtonManager : AImageButtonManager
    {
        [SerializeField] private SSoundSetting soundSetting;
        [SerializeField] private ButtonSoundPlayer soundPlayer;

        // どこから辿っても良いが、とにかくボタンのルートを取得して位置を算出する
        internal virtual RectTransform Parent => Image.rectTransform.parent as RectTransform;

        // ボタンの位置が動的に変わることはない想定なので、シンプルなキャッシュにする
        private bool hasCachedPosition = false;
        private Vector2 position;
        internal Vector2 Position
        {
            get
            {
                if (!hasCachedPosition)
                {
                    position = Parent.anchoredPosition;

                    hasCachedPosition = true;
                }
                return position;
            }
        }

        // ボタンの大きさが動的に変わることはない想定なので、シンプルなキャッシュにする
        private bool hasCachedSize = false;
        private Vector2 size;
        internal Vector2 Size
        {
            get
            {
                if (!hasCachedSize)
                {
                    size = Image.rectTransform.sizeDelta;

                    hasCachedSize = true;
                }
                return size;
            }
        }

        // ボタンの RaycastPadding が動的に変わることはない想定なので、シンプルなキャッシュにする
        private bool hasCachedRaycastPadding = false;
        private Vector4 raycastPadding;
        internal Vector4 RaycastPadding
        {
            get
            {
                if (!hasCachedRaycastPadding)
                {
                    raycastPadding = Image.raycastPadding;

                    hasCachedRaycastPadding = true;
                }
                return raycastPadding;
            }
        }

        private protected sealed override void PlayHoverSe()
        {
            base.PlayHoverSe();

            if (soundSetting.DoesPlayButtonHoverSe)
                soundPlayer.LetPlay(SButtonSound.Action.Hover);
        }

        private protected sealed override void PlayClickSe()
        {
            base.PlayClickSe();

            soundPlayer.LetPlay(SButtonSound.Action.Click);
        }
    }
}
