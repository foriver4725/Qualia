namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_GameRule", menuName = "SO/Game Rule")]
    internal sealed class SGameRule : ScriptableObject
    {
        [Serializable]
        internal sealed class ClearCondition
        {
            [SerializeField, Range(0.0f, 1.0e4f), Header("〇秒あるうち、")] private float maxElapse = 600.0f;
            [SerializeField, Range(0.0f, 1.0e4f), Header("〇秒経過でクリア (一緒の数値のはず)")] private float shouldElapse = 600.0f;
            internal float MaxElapse => maxElapse;
            internal float ShouldElapse => shouldElapse;

            [Space(30)]

            [SerializeField, Range(0, 50), Header("〇個あるうち、")] private byte maxFind = 10;
            [SerializeField, Range(0, 50), Header("〇個除去でクリア")] private byte shouldFind = 5;
            internal byte MaxFind => maxFind;
            internal byte ShouldFind => shouldFind;
        }

        [Serializable]
        internal sealed class DisasterOccurrenceConditions
        {
            [Serializable]
            internal sealed class SingleCondition
            {
                [SerializeField, Header("発生する災害")] private Disaster disaster = Disaster.None;
                internal Disaster Disaster => disaster;

                [Space(10)]

                [SerializeField, Range(0, 50), Header("〇個目を除去したら発生し、")] private byte beginCount = 2;
                [SerializeField, Range(0, 50), Header("△個目を除去するまたは、")] private byte endCount = 3;
                [SerializeField, Range(0.0f, 1.0e4f), Header("□秒経過したら終了する")] private float endDuration = 90.0f;
                internal byte BeginCount => beginCount;
                internal byte EndCount => endCount;
                internal float EndDuration => endDuration;
            }

            [SerializeField, Header("発生順に設定すること")] private SingleCondition[] conditions = Array.Empty<SingleCondition>();
            internal IReadOnlyList<SingleCondition> Conditions => conditions;
        }

        //　今後、複数ステージに増える予定
        [SerializeField] private ClearCondition clearCondition;
        [SerializeField] private DisasterOccurrenceConditions disasterOccurrenceConditions;

        internal ClearCondition GetClearCondition() => clearCondition;
        internal DisasterOccurrenceConditions GetDisasterOccurrenceConditions() => disasterOccurrenceConditions;
    }
}
