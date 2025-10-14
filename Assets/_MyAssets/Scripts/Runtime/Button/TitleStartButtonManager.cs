using MyScripts.Common.Button;

namespace MyScripts.Runtime
{
    internal sealed class TitleStartButtonManager : ASceneChangeButtonManager
    {
        private protected sealed override Scene TargetScene => Scene.Select;
    }
}
