using MyScripts.Common.Button;

namespace MyScripts.SO
{
    [CreateAssetMenu(fileName = "_ButtonColorSettings", menuName = "SO/OutGame/Button Color Settings")]
    internal sealed class SButtonColorSettings : ScriptableObject
    {
        internal enum Behaviour : byte
        {
            Normal,
            Notice,
        }

        [SerializeField] private ColorSettings normal;
        [SerializeField] private ColorSettings notice;

        internal ColorSettings Get(Behaviour behaviour) => behaviour switch
        {
            Behaviour.Normal => normal,
            Behaviour.Notice => notice,
            _ => throw new ArgumentOutOfRangeException(nameof(behaviour), behaviour, null)
        };
    }
}
