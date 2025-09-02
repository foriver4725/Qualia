namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_PlayerControlSound", menuName = "SO/Sound/InGame/Player Control")]
    internal sealed class SPlayerControlSound : ASSound
    {
        [SerializeField] private AudioClip inertiaJump;

        internal AudioClip InertiaJump => inertiaJump;
    }
}
