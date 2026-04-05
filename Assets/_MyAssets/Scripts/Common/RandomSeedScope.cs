namespace MyScripts.Common
{
    /// <summary>
    /// 一時的に乱数シードを変更する
    /// </summary>
    internal readonly struct RandomSeedScope : IDisposable
    {
        private readonly Random.State previousState;

        internal RandomSeedScope(int seed)
        {
            previousState = Random.state;
            Random.InitState(seed);
        }

        public readonly void Dispose()
        {
            Random.state = previousState;
        }
    }
}