namespace MyScripts.Runtime
{
    internal sealed class InGameSOHolder : ASingletonMonoBehaviour<InGameSOHolder>
    {
        [SerializeField] private SGameParameter gameParameter;
        [SerializeField] private SGameRule gameRule;
        [SerializeField] private SPlayerControl playerControl;
        [SerializeField] private SSOSSignLogText sosSignLogText;

        internal SGameParameter GameParameter => gameParameter;
        internal SGameRule GameRule => gameRule;
        internal SPlayerControl PlayerControl => playerControl;
        internal SSOSSignLogText SOSSignLogText => sosSignLogText;
    }
}
