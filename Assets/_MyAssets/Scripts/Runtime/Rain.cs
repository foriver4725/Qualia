namespace MyScripts.Runtime
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    internal sealed class Rain : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;

        private const int MaxPoint = 4096;
        private const float Speed = -1f;

        private const float Range = 32f;
        private const float RangeR = 1.0f / Range;

        private static readonly int Range_ID = Shader.PropertyToID("_Range");
        private static readonly int RangeR_ID = Shader.PropertyToID("_RangeR");
        private static readonly int MoveTotal_ID = Shader.PropertyToID("_MoveTotal");
        private static readonly int Move_ID = Shader.PropertyToID("_Move");
        private static readonly int TargetPosition_ID = Shader.PropertyToID("_TargetPosition");
        private static readonly int PrevInvMatrix_ID = Shader.PropertyToID("_PrevInvMatrix");

        private new Renderer renderer;

        private Vector3[] vertices;
        private int[] indices;
        private Color[] colors;
        private Vector2[] uvs;
        private float move = 0f;
        private Matrix4x4 viewMatrixPrev;

        private void Awake()
        {
            vertices = new Vector3[MaxPoint * 3];
            for (int i = 0; i < MaxPoint; i++)
            {
                float x = Random.Range(-Range, Range);
                float y = Random.Range(-Range, Range);
                float z = Random.Range(-Range, Range);
                var p = new Vector3(x, y, z);
                vertices[i * 3 + 0] = p;
                vertices[i * 3 + 1] = p;
                vertices[i * 3 + 2] = p;
            }

            indices = new int[MaxPoint * 3];
            for (int i = 0; i < MaxPoint * 3; i++)
            {
                indices[i] = i;
            }

            colors = new Color[MaxPoint * 3];
            for (int i = 0; i < MaxPoint; i++)
            {
                colors[i * 3 + 0] = new Color(1f, 1f, 1f, 0f);
                colors[i * 3 + 1] = new Color(1f, 1f, 1f, 1f);
                colors[i * 3 + 2] = new Color(1f, 1f, 1f, 0f);
            }

            uvs = new Vector2[MaxPoint * 3];
            for (int i = 0; i < MaxPoint; i++)
            {
                uvs[i * 3 + 0] = new Vector2(1f, 0f);
                uvs[i * 3 + 1] = new Vector2(1f, 0f);
                uvs[i * 3 + 2] = new Vector2(0f, 1f);
            }

            var mesh = new Mesh()
            {
                name = "RainMesh",
                vertices = vertices,
                colors = colors,
                uv = uvs,
                bounds = new Bounds(Vector3.zero, Vector3.one * 99999999)
            };

            renderer = GetComponent<Renderer>();

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            meshFilter.sharedMesh.SetIndices(indices, MeshTopology.Lines, 0);

            viewMatrixPrev = playerCamera.worldToCameraMatrix;

            renderer.material.SetFloat(Range_ID, Range);
            renderer.material.SetFloat(RangeR_ID, RangeR);
            renderer.material.SetFloat(Move_ID, Speed);
        }

        private void Update()
        {
            // Raindropがカメラの子なら、ターゲットはローカルでOK
            var target_position = new Vector3(0f, 0f, Range);

            renderer.material.SetFloat(MoveTotal_ID, move);
            renderer.material.SetVector(TargetPosition_ID, target_position);
            renderer.material.SetMatrix(PrevInvMatrix_ID, viewMatrixPrev * playerCamera.cameraToWorldMatrix);

            move = Mathf.Repeat(move + Speed, Range * 2f);
            viewMatrixPrev = playerCamera.worldToCameraMatrix;
        }
    }
}
