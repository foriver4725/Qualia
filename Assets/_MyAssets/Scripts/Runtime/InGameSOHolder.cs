namespace MyScripts.Runtime
{
    internal sealed class InGameSOHolder : ASingletonMonoBehaviour<InGameSOHolder>
    {
        [SerializeField] private SGameParameter gameParameter;
        [SerializeField] private SPlayerControl playerControl;
        [SerializeField] private SSOSSignLogText sosSignLogText;

        internal SGameParameter GameParameter => gameParameter;
        internal SPlayerControl PlayerControl => playerControl;
        internal SSOSSignLogText SOSSignLogText => sosSignLogText;
    }
}
