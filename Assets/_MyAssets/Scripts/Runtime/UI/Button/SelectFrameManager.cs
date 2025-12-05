namespace MyScripts.Runtime.UI.Button
{
    internal sealed class SelectFrameManager : ASingletonMonoBehaviour<SelectFrameManager>
    {
        [SerializeField] private Image image;
        [SerializeField] private RectTransform controllerIcon;

        // 9-sliced の外側8マスの幅 [px]
        // プログラム内で数値を決め打っておく
        private const int OuterSize = 32;

        internal AButtonManager SelectingButton { get; private set; } = null;

        /// <summary>
        /// 指定されたボタンが現在選択されているなら、その選択を解除して、何も選択していない状態にする<br/>
        /// 重複実行は気にしなくて良い<br/>
        /// </summary>
        internal void Deselect(AButtonManager button)
        {
            if (!SelectingButton) return;
            if (SelectingButton != button) return;

            image.gameObject.SetActive(false);
            SelectingButton = null;
        }

        /// <summary>
        /// 指定したボタンを新たに選択する<br/>
        /// 重複実行は気にしなくて良い<br/>
        /// </summary>
        internal void Reselect(AButtonManager button, float padding = 0.0f)
        {
            if (!SelectingButton)
                image.gameObject.SetActive(true);
            SelectingButton = button;

            image.rectTransform.anchoredPosition = button.Position;
            image.rectTransform.sizeDelta =
                button.Size // ボタンの基本サイズ
                - Vector2.up * (button.RaycastPadding.y + button.RaycastPadding.w) // RaycastPadding 分を引く (Left, Bottom, Right, Up)
                + Vector2.one * ((OuterSize + padding) * 2.0f); // 9-sliced の外側8マス分 + 余白 を足す
            controllerIcon.localPosition = (image.rectTransform.sizeDelta - Vector2.one * OuterSize) * 0.5f;
        }
    }
}
