namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_SoundSetting", menuName = "SO/OutGame/Sound Setting")]
    internal sealed class SSoundSetting : ScriptableObject
    {
        [SerializeField] private bool doesPlayButtonHoverSe = true;
        internal bool DoesPlayButtonHoverSe => doesPlayButtonHoverSe;
    }
}
