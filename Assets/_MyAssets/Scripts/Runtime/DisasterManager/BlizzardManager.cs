namespace MyScripts.Runtime
{
    internal sealed class BlizzardManager : ADisasterManager
    {
        private protected sealed override async UniTaskVoid Impl(Ct ct)
        {
            while (true)
            {
                await UniTask.WaitUntil(() => Enabled, cancellationToken: ct);

                "Windstorm started".Log();

                await UniTask.WaitUntil(() => !Enabled, cancellationToken: ct);

                "Windstorm ended".Log();
            }
        }
    }
}
