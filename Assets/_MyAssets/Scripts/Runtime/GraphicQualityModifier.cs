using UnityEngine.Rendering;

namespace MyScripts.Runtime
{
    internal sealed class GraphicQualityModifier : MonoBehaviour
    {
        [SerializeField] private RenderPipelineAsset low;
        [SerializeField] private RenderPipelineAsset medium;
        [SerializeField] private RenderPipelineAsset high;

        [SerializeField] private TextMeshProUGUI text;

        private void Update()
        {
            if (InputManager.DebugSetGraphicQualityLow.Bool)
            {
                UpdateRPAsset(low);
                UpdateText(text, "低");
            }
            else if (InputManager.DebugSetGraphicQualityMedium.Bool)
            {
                UpdateRPAsset(medium);
                UpdateText(text, "中");
            }
            else if (InputManager.DebugSetGraphicQualityHigh.Bool)
            {
                UpdateRPAsset(high);
                UpdateText(text, "高");
            }
        }

        private static void UpdateRPAsset(RenderPipelineAsset asset)
        {
            if (QualitySettings.renderPipeline != asset)
                QualitySettings.renderPipeline = asset;
        }

        private static void UpdateText(TextMeshProUGUI text, string constWord)
        {
            text.SetTextFormat("現在のグラフィック品質 : <color=#00ffff>{0}</color>", constWord);
        }
    }
}
