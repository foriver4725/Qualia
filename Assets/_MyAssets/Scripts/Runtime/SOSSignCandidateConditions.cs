namespace MyScripts.Runtime
{
    /// <summary>
    /// それぞれの SOSサイン 配置候補箇所にアタッチする
    /// </summary>
    internal sealed class SOSSignCandidateConditions : MonoBehaviour
    {
        internal interface ICompareInput
        {
            Difficulty CurrentDifficulty { get; }
        }

        [SerializeField, Tooltip("この難易度以上なら配置できる")] private Difficulty minDifficulty = Difficulty.Easy;

        internal bool CanPlace(ICompareInput input)
        {
            if (input.CurrentDifficulty < minDifficulty)
                return false;

            return true;
        }
    }
}
