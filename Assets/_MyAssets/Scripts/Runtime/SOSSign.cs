namespace MyScripts.Runtime
{
    internal sealed class SOSSign : MonoBehaviour
    {
        [SerializeField] private CharacterType characterType = CharacterType.Land;
        [SerializeField] private GameObject[] landObjects;
        [SerializeField] private GameObject[] seaObjects;
        [SerializeField] private ParticleSystem smokeParticle;

        [SerializeField] private new Collider collider;

        internal Collider Collider => collider;

        private void Awake()
        {
            if (characterType is CharacterType.None)
                return;

            // 自身の子供の中からランダムなオブジェクトを選択して、
            // それのみ有効化・他は全部無効化する
            {
                Dictionary<CharacterType, GameObject[]> typeObjectsMap = new()
                {
                    { CharacterType.Land, landObjects },
                    { CharacterType.Sea, seaObjects },
                    { CharacterType.Sky, Array.Empty<GameObject>() },
                };

                // 自身のオブジェクト群の中で、このインデックスのオブジェクトのみ有効化する
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

            // 煙パーティクルの設定
            {
                switch (characterType)
                {
                    case CharacterType.Land:
                        {
                            smokeParticle.gameObject.SetActive(false);
                            var main = smokeParticle.main;
                            main.startLifetime = new(1.0f, 2.0f);
                        }
                        break;
                    case CharacterType.Sea:
                        {
                            smokeParticle.gameObject.SetActive(false);
                        }
                        break;
                    case CharacterType.Sky:
                        {
                            smokeParticle.gameObject.SetActive(true);
                            var main = smokeParticle.main;
                            main.startLifetime = new(3.0f, 4.0f);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
