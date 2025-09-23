namespace MyScripts.Runtime
{
    internal sealed class SelectSceneManager : MonoBehaviour, IOutGameSceneManagerMultiTransition
    {
        [SerializeField] private Transform selectButtonsRoot; //! 配下には、ボタンのみがステージ順に配置されている前提
        [SerializeField] private Button backButton;

        private void Awake() => Impl(destroyCancellationToken).Forget();

        private async UniTaskVoid Impl(Ct ct)
        {
            int buttonsAmount = selectButtonsRoot.childCount;

            UniTask[] tasks = new UniTask[buttonsAmount + 1]; // 最初に back ボタン、それ以降に select ボタン
            tasks[0] = backButton.OnClickAsync(ct);
            for (int i = 0; i < buttonsAmount; i++)
            {
                int index = i;
                Button button = selectButtonsRoot.GetChild(index).GetComponent<Button>();
                button.GetComponentInChildren<TextMeshProUGUI>().SetTextFormat("ステージ {0}", index + 1);
                tasks[index + 1] = button.OnClickAsync(ct);
            }

            int completedTaskIndex = await UniTask.WhenAny(tasks);

            Scene nextScene = (completedTaskIndex == 0) ? Scene.Title : Scene.Main; //! 仮で Main に飛ぶようにしておく
            TransitToScene(nextScene);
        }

        public void TransitToScene(Scene scene) => LoadManager.Instance.BeginLoad(scene);
    }
}
