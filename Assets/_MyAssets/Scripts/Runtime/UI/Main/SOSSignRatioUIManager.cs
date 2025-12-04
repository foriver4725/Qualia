namespace MyScripts.Runtime.UI.Main
{
    internal sealed class SOSSignRatioUIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform fillRect;
        [SerializeField] private Image p25Mark;
        [SerializeField] private Image p50Mark;
        [SerializeField] private Image p75Mark;
        [Space(10)]
        [SerializeField, Range(0.0f, 256.0f)] private float markSizeWhenActive = 54.0f;
        [SerializeField, Range(0.0f, 256.0f)] private float markSizeWhenNotActive = 36.0f;
        [SerializeField, ColorUsage(showAlpha: false)] private Color32 markColorWhenActive = new(115, 188, 104, 255);
        [SerializeField, ColorUsage(showAlpha: false)] private Color32 markColorWhenNotActive = new(34, 106, 22, 255);

        // Awake で初期化
        private float fillRectWidthInit;

        private void Awake()
        {
            fillRectWidthInit = fillRect.sizeDelta.x;
        }

        // 引数は [0.0f, 1.0f]
        //! 初期値をキャッシュする関係で、必ず Awake() より後に呼び出すこと!!
        internal void UpdateUI(float leftRatio)
        {
            text.SetTextFormat("穢れ度 : {0:F2}%", leftRatio * 100.0f);

            fillRect.SetWidth(fillRectWidthInit * leftRatio);

            SetMarkView(p25Mark, leftRatio >= 0.25f);
            SetMarkView(p50Mark, leftRatio >= 0.50f);
            SetMarkView(p75Mark, leftRatio >= 0.75f);
        }

        private void SetMarkView(Image mark, bool isActive)
        {
            if (isActive)
            {
                mark.rectTransform.sizeDelta = Vector2.one * markSizeWhenActive;
                mark.color = markColorWhenActive;
            }
            else
            {
                mark.rectTransform.sizeDelta = Vector2.one * markSizeWhenNotActive;
                mark.color = markColorWhenNotActive;
            }
        }
    }
}
