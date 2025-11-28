namespace MyScripts.Runtime.UI.Button
{
    internal sealed class SelectFrameManager : ASingletonMonoBehaviour<SelectFrameManager>
    {
        [SerializeField] private Image image;

        private bool isSelectingAny = false;

        /// <summary>
        /// 何も選択していない状態にする
        /// </summary>
        internal void Deselect()
        {
            if (!isSelectingAny) return;
            isSelectingAny = false;

            image.gameObject.SetActive(false);
        }

        /// <summary>
        /// 指定したボタンを新たに選択する
        /// </summary>
        internal void Reselect(AButtonManager button, float padding = 20.0f)
        {
            if (!isSelectingAny)
            {
                isSelectingAny = true;
                image.gameObject.SetActive(true);
            }

            image.rectTransform.anchoredPosition = button.Position;
            image.rectTransform.sizeDelta = button.Size + new Vector2(padding, padding) * 2.0f;
        }
    }
}
