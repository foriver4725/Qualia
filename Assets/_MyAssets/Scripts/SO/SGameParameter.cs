namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_GameParameter", menuName = "SO/Game Parameter")]
    internal sealed class SGameParameter : ScriptableObject
    {
        [SerializeField] private BarrierBorderSettings barrierBorder;
        internal BarrierBorderSettings BarrierBorder => barrierBorder;
        [Serializable]
        internal sealed class BarrierBorderSettings
        {
            [SerializeField, MinMaxRange(0.0f, 1000.0f), Tooltip("透明度が変化する距離区間(プレイヤーとの距離) [m]\n段々と見えるようになり、完全に表示する")] private Vector2 alphaChangingRange = new(5.0f, 50.0f);
            internal float AlphaChangingDistanceMin => alphaChangingRange.x;
            internal float AlphaChangingDistanceMax => alphaChangingRange.y;
        }
    }
}
