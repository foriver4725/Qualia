namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_GameRule", menuName = "SO/Game Rule")]
    internal sealed class SGameRule : ScriptableObject
    {
        internal interface IClearCondition
        {
            // 〇秒経過でクリア
            float ShouldElapse { get; }
            // 最大値
            float MaxElapse { get; }

            // 〇個除去でクリア
            byte ShouldFind { get; }
            // 最大値
            byte MaxFind { get; }
        }

        [Serializable]
        internal sealed class ClearCondition : IClearCondition
        {
            [SerializeField, Range(0.0f, 1.0e4f), Header("〇秒あるうち、")] private float maxElapse = 600.0f;
            [SerializeField, Range(0.0f, 1.0e4f), Header("〇秒経過でクリア (一緒の数値のはず)")] private float shouldElapse = 600.0f;
            [Space(30)]
            [SerializeField, Range(0, 50), Header("〇個あるうち、")] private byte maxFind = 10;
            [SerializeField, Range(0, 50), Header("〇個除去でクリア")] private byte shouldFind = 5;

            public float ShouldElapse => shouldElapse;
            public float MaxElapse => maxElapse;

            public byte ShouldFind => shouldFind;
            public byte MaxFind => maxFind;
        }

        //　今後、複数ステージに増える予定
        [SerializeField] private ClearCondition clearCondition;

        internal IClearCondition GetClearCondition() => clearCondition;
    }
}
