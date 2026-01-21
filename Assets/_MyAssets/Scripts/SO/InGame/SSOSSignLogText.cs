namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_SOSSignLogText", menuName = "SO/InGame/SOS Sign Log Text")]
    internal sealed class SSOSSignLogText : ScriptableObject
    {
        [Header("複数のログテキストの中から、ランダムに表示する")]

        [Header("SOSサインと接触・アニマ未所持")]
        [SerializeField, TextArea(1, 1000)] private string[] onTouchWithoutAnima = new string[0];
        [Header("SOSサインと接触・アニマ所持")]
        [SerializeField, TextArea(1, 1000)] private string[] onTouchWithAnima = new string[0];

        [Header("SOSサインを取り除こうとした (アニマ未所持)")]
        [SerializeField, TextArea(1, 1000)] private string[] onRemoveWithoutAnima = new string[0];
        [Header("SOSサインを取り除いた (アニマ所持)")]
        [SerializeField, TextArea(1, 1000)] private string[] onRemoveWithAnima = new string[0];

        internal enum LogType : byte
        {
            OnTouchWithoutAnima,
            OnTouchWithAnima,

            OnRemoveWithoutAnima,
            OnRemoveWithAnima,
        }

        internal string GetRandom(LogType logType)
        {
            string[] texts = logType switch
            {
                LogType.OnTouchWithoutAnima => onTouchWithoutAnima,
                LogType.OnTouchWithAnima => onTouchWithAnima,

                LogType.OnRemoveWithoutAnima => onRemoveWithoutAnima,
                LogType.OnRemoveWithAnima => onRemoveWithAnima,

                _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null),
            };

            int length = texts.Length;
            if (length <= 0)
            {
                $"{nameof(texts)} must not be empty. logType: {logType}".Print(LogSettings.Warning);
                return string.Empty;
            }

            return texts[Random.Range(0, length)];
        }
    }
}
