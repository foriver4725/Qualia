namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_SOSSign", menuName = "SO/InGame/SOS Sign")]
    internal sealed class SSOSSign : ScriptableObject
    {
        [SerializeField] private GameObject[] landPrefabs;
        [SerializeField] private GameObject[] seaPrefabs;

        internal GameObject GetRandom(CharacterType type)
        {
            GameObject[] prefabs = type switch
            {
                CharacterType.Land => landPrefabs,
                CharacterType.Sea => seaPrefabs,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            int length = prefabs.Length;
            if (length <= 0)
            {
                $"{nameof(prefabs)} must not be empty. type: {type}".Print(LogSettings.Warning);
                return null;
            }

            return prefabs[Random.Range(0, length)];
        }
    }
}
