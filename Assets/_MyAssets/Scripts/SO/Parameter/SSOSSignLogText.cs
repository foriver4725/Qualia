namespace MyScripts.SO.Parameter
{
    [CreateAssetMenu(fileName = "_SOSSignLogText", menuName = "SO/Parameter/SOSSignLogText")]
    internal sealed class SSOSSignLogText : ScriptableObject
    {
        [Header("動物時 : 近づいた (ランダムに表示)")]
        [SerializeField, TextArea(1, 1000)] private string[] onAnimalApproach = new string[0];

        [Header("人間時 : クリックした (ランダムに表示)")]
        [SerializeField, TextArea(1, 1000)] private string[] onHumanClick = new string[0];

        internal enum LogType : byte
        {
            OnAnimalApproach,
            OnHumanClick,
        }

        internal string GetRandom(LogType logType)
        {
            string[] texts = logType switch
            {
                LogType.OnAnimalApproach => onAnimalApproach,
                LogType.OnHumanClick => onHumanClick,
                _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
            };

            int length = texts.Length;
            if (length <= 0)
            {
                $"{nameof(texts)} must not be empty. logType: {logType}".LogWarning();
                return string.Empty;
            }

            return texts[Random.Range(0, length)];
        }
    }
}