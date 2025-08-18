namespace MyScripts.Runtime
{
    internal sealed class BarrierBordersManager : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] barrierBorders;
        [SerializeField, Tooltip("プレイヤーとの距離に応じて可視状態を変える\nx が完全不透明にする距離, y が完全透明にする距離")]
        private Vector2 playerDistLimits = new(5.0f, 50.0f);
        [SerializeField] private Transform playerBody;

        // Awake で初期化
        private MaterialPropertyBlock[] propertyBlocks;
        private float playerDistLimitMinSqr;
        private float playerDistLimitMaxSqr;

        private static readonly int EnabledID = Shader.PropertyToID("_Enabled");
        private static readonly int TransparencyID = Shader.PropertyToID("_TransparencyCoefficient");

        private void Awake()
        {
            propertyBlocks = new MaterialPropertyBlock[barrierBorders.Length];
            for (int i = 0; i < barrierBorders.Length; i++)
                propertyBlocks[i] = new();

            playerDistLimitMinSqr = playerDistLimits.x * playerDistLimits.x;
            playerDistLimitMaxSqr = playerDistLimits.y * playerDistLimits.y;
        }

        private void Start()
        {
            Impl(destroyCancellationToken).Forget();
        }

        private async UniTaskVoid Impl(Ct ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (playerBody != null)
                {
                    for (int i = 0; i < barrierBorders.Length; i++)
                    {
                        var border = barrierBorders[i];
                        if (border == null) continue;

                        // プレイヤーとの距離を計算 (XZ)
                        float distSqr = CalcDistSqr(border.transform, playerBody.position);

                        // プロパティ値を算出
                        (bool enabled, float transparency) = distSqr switch
                        {
                            _ when distSqr <= playerDistLimitMinSqr => (true, 1.0f), // 完全不透明
                            _ when distSqr >= playerDistLimitMaxSqr => (false, 0.0f), // 完全透明
                            _ => (true, distSqr.Remap(playerDistLimitMinSqr, playerDistLimitMaxSqr, 1.0f, 0.0f)) // 中間の透明度
                        };

                        // プロパティブロックに値を設定
                        var block = propertyBlocks[i];
                        border.GetPropertyBlock(block);
                        block.SetFloat(EnabledID, enabled ? 1.0f : 0.0f);
                        block.SetFloat(TransparencyID, transparency);
                        border.SetPropertyBlock(block);
                    }
                }

                // ずっとは処理が重たいので、16フレーム毎にする
                await UniTask.DelayFrame(16, cancellationToken: ct);
            }
        }

        private float CalcDistSqr(Transform border, Vector3 playerPos)
        {
            // 参考 : http://marupeke296.com/COL_2D_No5_PolygonToCircle.html

            float borderScaleX = border.lossyScale.x;
            Vector3 borderStart3D = border.position - border.right * (borderScaleX * 0.5f);
            Vector3 borderEnd3D = border.position + border.right * (borderScaleX * 0.5f);
            Vector2 borderStartXZ = new(borderStart3D.x, borderStart3D.z);
            Vector2 borderEndXZ = new(borderEnd3D.x, borderEnd3D.z);

            Vector2 playerPosXZ = new(playerPos.x, playerPos.z);

            Vector2 S = borderEndXZ - borderStartXZ;
            Vector2 A = playerPosXZ - borderStartXZ;
            Vector2 B = playerPosXZ - borderEndXZ;

            bool isOuterSegStart = Vector2.Dot(A, S) <= 0;
            bool isOuterSegEnd = Vector2.Dot(B, S) >= 0;

            return 0 switch
            {
                _ when isOuterSegStart => A.sqrMagnitude,
                _ when isOuterSegEnd => B.sqrMagnitude,
                _ => S.CrossSqr(A) / S.sqrMagnitude
            };
        }
    }
}