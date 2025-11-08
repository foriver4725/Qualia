namespace MyScripts.Runtime
{
    internal sealed class CharacterShaderColorsSynchronizer : MonoBehaviour
    {
        [SerializeField] private Material[] shaderMaterials;

        private static readonly int GlobalTimeID = Shader.PropertyToID("_GlobalTime");

        // Awake で初期化
        private float[] initialGlobalTimes;

        private void Awake()
        {
            initialGlobalTimes = new float[shaderMaterials.Length];
            for (int i = 0; i < shaderMaterials.Length; i++)
            {
                initialGlobalTimes[i] = shaderMaterials[i].GetFloat(GlobalTimeID);
            }
        }

        private void Update()
        {
            float time = Time.time;
            foreach (var material in shaderMaterials)
            {
                material.SetFloat(GlobalTimeID, time);
            }
        }

        private void OnDestroy()
        {
            // 差分が発生しないように、初期値に戻す
            for (int i = 0; i < shaderMaterials.Length; i++)
            {
                shaderMaterials[i].SetFloat(GlobalTimeID, initialGlobalTimes[i]);
            }
        }
    }
}
