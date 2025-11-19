namespace MyScripts.Runtime
{
    internal sealed class SelectDifficultyAndSceneChangeButtonManager : SceneChangeButtonManager
    {
        [SerializeField] private Difficulty difficulty;

        private protected sealed override void InvokeLoad()
        {
            GlobalValues.Difficulty = difficulty;
            base.InvokeLoad();
        }
    }
}
