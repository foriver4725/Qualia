namespace MyScripts.Runtime
{
    internal sealed class BarrierBordersManager : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] barrierBorders;
        [SerializeField, MinMaxRange(0.0f, 1000.0f), Tooltip("透明度が変化する距離区間(プレイヤーとの距離) [m]\n段々と見えるようになり、完全に表示する")]
        private Vector2 alphaChangingRange = new(5.0f, 50.0f);
        [SerializeField] private Transform playerBody;

        // Awake で初期化
        private MaterialPropertyBlock mpb;
        private float alphaChangingDistSqrMin;
        private float alphaChangingDistSqrMax;

        private static readonly int WholeTransparencyID = Shader.PropertyToID("_WholeTransparency");

        private void Awake()
        {
            mpb = new();

            alphaChangingDistSqrMin = alphaChangingRange.x * alphaChangingRange.x;
            alphaChangingDistSqrMax = alphaChangingRange.y * alphaChangingRange.y;
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
                        (bool enabled, float alpha) = distSqr switch
                        {
                            _ when distSqr <= alphaChangingDistSqrMin => (true, 1.0f), // 完全不透明
                            _ when distSqr >= alphaChangingDistSqrMax => (false, 0.0f), // 完全透明
                            _ => (true, distSqr.Remap(alphaChangingDistSqrMin, alphaChangingDistSqrMax, 1.0f, 0.0f)) // 中間の透明度
                        };

                        // コンポーネントの有効/無効を切り替え
                        // 無効にするなら、以降の処理はスキップ
                        if (enabled ^ border.enabled)
                            border.enabled = enabled;
                        if (!enabled)
                            continue;

                        // プロパティブロックに値を設定
                        border.GetPropertyBlock(mpb);
                        mpb.SetFloat(WholeTransparencyID, alpha);
                        border.SetPropertyBlock(mpb);
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
