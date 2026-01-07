using UnityEngine.Video;

namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_CutScene", menuName = "SO/InGame/CutScene")]
    internal sealed class SCutScene : ScriptableObject
    {
        internal enum CutSceneType : byte
        {
            Intro,         // オープニング
            AnimaDesc,     // アニマの説明
            SOSRemoveDesc, // SOSサイン除去の説明
        }

        [SerializeField] private VideoClip introClip;
        [SerializeField] private VideoClip animaDescClip;
        [SerializeField] private VideoClip sosRemoveDescClip;

        internal VideoClip Get(CutSceneType type) => type switch
        {
            CutSceneType.Intro => introClip,
            CutSceneType.AnimaDesc => animaDescClip,
            CutSceneType.SOSRemoveDesc => sosRemoveDescClip,
            _ => throw new System.ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
