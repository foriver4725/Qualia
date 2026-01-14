namespace MyScripts.Runtime
{
    internal sealed class SOSSign : MonoBehaviour
    {
        [SerializeField] private CharacterType characterType = CharacterType.Land;
        [SerializeField] private GameObject[] landObjects;
        [SerializeField] private GameObject[] seaObjects;
        [SerializeField] private GameObject[] skyObjects;

        [SerializeField] private new Collider collider;

        internal Collider Collider => collider;

        private void Awake()
        {
            // 自身の子供の中からランダムなオブジェクトを選択して、
            // それのみ有効化・他は全部無効化する

            Dictionary<CharacterType, GameObject[]> typeObjectsMap = new()
            {
                { CharacterType.Land, landObjects },
                { CharacterType.Sea, seaObjects },
                { CharacterType.Sky, skyObjects },
            };

            if (characterType is not CharacterType.None)
            {
                // 自身のオブジェクト群の中で、このインデックスのオブジェクトのみ、有効化する
                int objectToUseIndex = Random.Range(0, typeObjectsMap[characterType].Length);

                foreach (var (type, objects) in typeObjectsMap)
                {
                    for (int i = 0; i < objects.Length; i++)
                    {
                        bool isActive = (type == characterType && i == objectToUseIndex);

                        objects[i].SetActive(isActive);
                        // ランダムに回転させて、自然な感じにする
                        if (isActive)
                            objects[i].transform.SetLocalRotY(Random.Range(0f, 360f));
                    }
                }
            }
        }
    }
}
