namespace MyScripts.Runtime.UI.Button
{
    internal sealed class SelectFrameManager : ASingletonMonoBehaviour<SelectFrameManager>
    {
        [SerializeField] private Image image;

        internal AButtonManager SelectingButton { get; private set; } = null;

        /// <summary>
        /// 何も選択していない状態にする<br/>
        /// 重複実行は気にしなくて良い<br/>
        /// </summary>
        internal void Deselect()
        {
            if (SelectingButton)
                image.gameObject.SetActive(false);
            SelectingButton = null;
        }

        /// <summary>
        /// 指定したボタンを新たに選択する<br/>
        /// 重複実行は気にしなくて良い<br/>
        /// </summary>
        internal void Reselect(AButtonManager button, float padding = 20.0f)
        {
            if (!SelectingButton)
                image.gameObject.SetActive(true);
            SelectingButton = button;

            image.rectTransform.anchoredPosition = button.Position;
            image.rectTransform.sizeDelta = button.Size + new Vector2(padding, padding) * 2.0f;
        }
    }
}
