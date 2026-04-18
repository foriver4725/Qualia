using UnityEngine.Video;

namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_StoryMovie", menuName = "SO/Movie/Story Movie")]
    internal sealed class SStoryMovie : ScriptableObject
    {
        internal enum GameProgress : byte
        {
            P0,
            P33,
            P66,
            P100,
        }

        [SerializeField] private VideoClip p0;
        [SerializeField] private VideoClip p33;
        [SerializeField] private VideoClip p66;
        [SerializeField] private VideoClip p100;

        internal VideoClip Get(GameProgress progress) => progress switch
        {
            GameProgress.P0   => p0,
            GameProgress.P33  => p33,
            GameProgress.P66  => p66,
            GameProgress.P100 => p100,
            _                 => throw new ArgumentOutOfRangeException(nameof(progress), progress, null)
        };
    }
}