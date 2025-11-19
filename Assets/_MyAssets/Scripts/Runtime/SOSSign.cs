namespace MyScripts.Runtime
{
    internal sealed class SOSSign : MonoBehaviour
    {
        [SerializeField] private new ParticleSystem particleSystem = null;
        [SerializeField] private new Collider collider = null;

        internal ParticleSystem ParticleSystem => particleSystem;
        internal Collider Collider => collider;
    }
}
