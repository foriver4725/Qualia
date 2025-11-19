namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_GameRule", menuName = "SO/InGame/Game Rule")]
    internal sealed class SGameRule : ScriptableObject
    {
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

        //　今後、複数ステージに増える予定
        [SerializeField, Range(0.0f, 1.0e4f), Header("制限時間 [秒]")] private float timeLimit = 600.0f;
        [SerializeField, Header("発生順に設定すること")] private DisasterOccurrenceCondition[] disasterOccurrenceConditions;

        internal float TimeLimit => timeLimit;
        internal IReadOnlyList<DisasterOccurrenceCondition> GetDisasterOccurrenceConditions() => disasterOccurrenceConditions;
    }
}
