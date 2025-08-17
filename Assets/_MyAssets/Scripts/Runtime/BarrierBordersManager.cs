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
                        Vector3 diff = border.transform.position - playerBody.position;
                        Vector2 diffXZ = new(diff.x, diff.z);
                        float distSqr = diffXZ.sqrMagnitude;

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
    }
}