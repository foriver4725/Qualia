namespace MyScripts.Runtime
{
    /// <summary>
    /// <para>Image コンポーネントに設定されているパノラマ画像を、ずっと回転させる</para>
    /// <para>事前にパノラマ画像を設定しておくこと</para>
    /// <para>マテリアルのUV座標のうち、U座標を時間経過で変化させることで実現</para>
    /// </summary>
    internal sealed class PanoramaImageRotator : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField, Range(0.0f, 1.0f)] private float rotateSpeed = 0.02f; // 1秒あたりのU変化量

        private static readonly int MainTexST_ID = Shader.PropertyToID("_MainTex_ST");

        private void Update()
        {
            // _MainTex_ST = (scale.x, scale.y, offset.x, offset.y)
            Vector4 st = image.material.GetVector(MainTexST_ID);

            // offset.x が U座標のオフセットに対応
            st.z = Mathf.Repeat(st.z + rotateSpeed * Time.deltaTime, 1.0f);
            image.material.SetVector(MainTexST_ID, st);
        }
    }
}
