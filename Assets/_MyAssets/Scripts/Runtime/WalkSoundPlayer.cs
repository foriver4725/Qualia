using MyScripts.SO.Reference;

namespace MyScripts.Runtime
{
    internal sealed class WalkSoundPlayer : MonoBehaviour
    {
        [SerializeField] private SWalkSound walkSoundRef;
        [SerializeField] private Transform walkSoundRoot;
    }
}
