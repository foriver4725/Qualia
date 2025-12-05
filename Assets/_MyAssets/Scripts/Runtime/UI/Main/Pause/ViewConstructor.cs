using MyScripts.Runtime.UI.Button;

namespace MyScripts.Runtime.UI.Main.Pause
{
    // UIが有効になるたびに実行するべき
    // 現在の数値を基に、見た目を再構成する
    internal sealed class ViewConstructor : ASingletonMonoBehaviour<ViewConstructor> // ポーズはプレイヤー側から呼ばれるので、シングルトンにしてしまう
    {
        // 最初に選択されるもの
        [SerializeField] private ResumeButtonManager resumeButtonManager;

        internal void Construct()
        {
            SelectFrameManager.Instance.Reselect(resumeButtonManager);
        }
    }
}
