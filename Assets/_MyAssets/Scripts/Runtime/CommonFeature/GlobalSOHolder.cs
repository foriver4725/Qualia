namespace MyScripts.Runtime
{
    internal sealed class GlobalSOHolder : ASingletonMonoBehaviour<GlobalSOHolder>
    {
        [SerializeField] private SGameRule gameRule;

        internal SGameRule GameRule => gameRule;
    }
}
