namespace MyScripts.Runtime
{
    internal sealed class SelectDifficultyAndSceneChangeButtonManager : SceneChangeButtonManager
    {
        [SerializeField] private Difficulty difficulty;

        private protected sealed override void InvokeLoad()
        {
            SOSSignFindManager.PlaceAmount = difficulty switch
            {
                Difficulty.Easy => 3,
                Difficulty.Normal => 5,
                Difficulty.Hard => 10,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
            };

            base.InvokeLoad();
        }
    }
}
