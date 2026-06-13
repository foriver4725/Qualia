using AK.Wwise;

namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_SOSSound", menuName = "SO/Sound/InGame/SOS")]
    internal sealed class SSOSSound : ASSoundWithType<SSOSSound.Situation>
    {
        [SerializeField] private AudioClip couldRemove;
        [SerializeField] private AudioClip couldNotRemove;
        [SerializeField] private AK.Wwise.Event Play_SOS;
        [SerializeField] private AK.Wwise.Switch Remove;
        [SerializeField] private AK.Wwise.Switch NotRemove;

        //internal AK.Wwise.Event PostEvent => postEvent;


        internal enum Situation : byte
        {
            CouldRemove,
            CouldNotRemove,
        }


        internal sealed override AudioClip GetClip(Situation type) => type switch
        {
            Situation.CouldRemove => couldRemove,
            Situation.CouldNotRemove => couldNotRemove,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        internal sealed override AK.Wwise.Switch GetSwitch(Situation type) => type switch
        {
            Situation.CouldRemove => Remove,
            Situation.CouldNotRemove => NotRemove,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        internal sealed override AK.Wwise.Event GetEvent()
        {
            return Play_SOS;
        }

    }
}
