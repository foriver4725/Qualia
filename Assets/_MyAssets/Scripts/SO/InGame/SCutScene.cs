using UnityEngine.Video;

namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_CutScene", menuName = "SO/InGame/CutScene")]
    internal sealed class SCutScene : ScriptableObject
    {
        internal enum CutSceneType : byte
        {
            Intro,         // オープニング
            AnimaDesc,     // アニマの説明と、ゲームの遊び方
        }

        [SerializeField] private VideoClip introClip;
        [SerializeField] private VideoClip animaDescClip;

        internal VideoClip Get(CutSceneType type) => type switch
        {
            CutSceneType.Intro => introClip,
            CutSceneType.AnimaDesc => animaDescClip,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
