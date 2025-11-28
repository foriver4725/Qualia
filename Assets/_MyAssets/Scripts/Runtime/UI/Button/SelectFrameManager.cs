namespace MyScripts.Runtime.UI.Button
{
    internal sealed class SelectFrameManager : ASingletonMonoBehaviour<SelectFrameManager>
    {
        [SerializeField] private Image image;

        internal void SetActive(bool value) => image.gameObject.SetActive(value);
        internal void SetPosition(Vector2 position) => image.rectTransform.anchoredPosition = position;
        internal void SetScale(Vector2 scale) => image.rectTransform.sizeDelta = scale;
    }
}
