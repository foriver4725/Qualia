using UnityEngine.Rendering;

namespace MyScripts.Runtime
{
    internal sealed class GraphicQualityModifier : MonoBehaviour
    {
        [SerializeField] private RenderPipelineAsset low;
        [SerializeField] private RenderPipelineAsset medium;
        [SerializeField] private RenderPipelineAsset high;

        private void Update()
        {
            if (InputManager.DebugSetGraphicQualityLow.Bool)
            {
                UpdateRPAsset(low);
            }
            else if (InputManager.DebugSetGraphicQualityMedium.Bool)
            {
                UpdateRPAsset(medium);
            }
            else if (InputManager.DebugSetGraphicQualityHigh.Bool)
            {
                UpdateRPAsset(high);
            }
        }

        private static void UpdateRPAsset(RenderPipelineAsset asset)
        {
            if (QualitySettings.renderPipeline != asset)
                QualitySettings.renderPipeline = asset;
        }
    }
}
