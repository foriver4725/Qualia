namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_GameRule", menuName = "SO/InGame/Game Rule")]
    internal sealed class SGameRule : ScriptableObject
    {
        [Serializable]
        internal sealed class SOSSignMaxAmountsLiterally
        {
            [SerializeField, Range(1, 50), Header($"見つけるべきSOSサインの数 ({nameof(Difficulty.Easy)})")] private byte easy;
            [SerializeField, Range(1, 50), Header($"見つけるべきSOSサインの数 ({nameof(Difficulty.Normal)})")] private byte normal;
            [SerializeField, Range(1, 50), Header($"見つけるべきSOSサインの数 ({nameof(Difficulty.Hard)})")] private byte hard;

            internal byte Get(Difficulty difficulty) => difficulty switch
            {
                Difficulty.Easy => easy,
                Difficulty.Normal => normal,
                Difficulty.Hard => hard,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
            };
        }

        [Serializable]
        internal sealed class DisasterOccurrenceCondition
        {
            [SerializeField, Header("発生する災害")] private Disaster disaster = default;
            internal Disaster Disaster => disaster;

            [Space(10)]

            [SerializeField, Range(0, 50), Header("〇個目を除去したら発生し、")] private byte beginCount = 2;
            [SerializeField, Range(0, 50), Header("△個目を除去するまたは、")] private byte endCount = 3;
            [SerializeField, Range(0.0f, 1.0e4f), Header("□秒経過したら終了する")] private float endDuration = 90.0f;
            internal byte BeginCount => beginCount;
            internal byte EndCount => endCount;
            internal float EndDuration => endDuration;
        }

        [SerializeField, Range(0.0f, 1.0e4f), Header("制限時間 [秒]")] private float timeLimit = 600.0f;
        [SerializeField, Header("見つけるべきSOSサインの数")] private SOSSignMaxAmountsLiterally sosSignMaxAmounts;
        [SerializeField, Header("発生順に設定すること")] private DisasterOccurrenceCondition[] disasterOccurrenceConditions;

        internal float TimeLimit => timeLimit;
        internal SOSSignMaxAmountsLiterally SOSSignMaxAmounts => sosSignMaxAmounts;
        internal IReadOnlyList<DisasterOccurrenceCondition> DisasterOccurrenceConditions => disasterOccurrenceConditions;
    }
}
