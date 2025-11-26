namespace MyScripts.Runtime
{
    internal sealed class InGameSOHolder : ASingletonMonoBehaviour<InGameSOHolder>
    {
        [SerializeField] private SGameRule gameRule;
        [SerializeField] private SGameParameter gameParameter;
        [SerializeField] private SPlayerControl playerControl;
        [SerializeField] private SSOSSignLogText sosSignLogText;

        internal SGameRule GameRule => gameRule;
        internal SGameParameter GameParameter => gameParameter;
        internal SPlayerControl PlayerControl => playerControl;
        internal SSOSSignLogText SOSSignLogText => sosSignLogText;
    }
}
