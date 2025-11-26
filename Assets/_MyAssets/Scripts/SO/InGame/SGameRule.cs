namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_GameRule", menuName = "SO/InGame/Game Rule")]
    internal sealed class SGameRule : ScriptableObject
    {
        [SerializeField, Range(0, 1000)] private int sosSignCount = 100;
        internal int SOSSignCount => sosSignCount;
    }
}
