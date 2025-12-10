namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_Anima", menuName = "SO/InGame/Anima")]
    internal sealed class SAnima : ScriptableObject
    {
        [SerializeField] private Sprite landIcon;
        [SerializeField] private Sprite seaIcon;
        [SerializeField] private Sprite skyIcon;
        internal Sprite LandIcon => landIcon;
        internal Sprite SeaIcon => seaIcon;
        internal Sprite SkyIcon => skyIcon;

        [SerializeField, Range(0.0f, 600.0f), Tooltip("この秒数経過したら、取得状態が終了する")] private float possessDuration = 30.0f;
        internal float PossessDuration => possessDuration;
    }
}
