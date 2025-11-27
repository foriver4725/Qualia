namespace MyScripts.Runtime.UI.Main.Pause
{
    /// <summary>
    /// 確認UIの決定ボタンなので、ある種マネージャーみたいな役割も担う
    /// </summary>
    internal sealed class ConfirmYesButtonManager : AButtonManager
    {
        [SerializeField] private TextMeshProUGUI confirmLabelText;

        internal enum InvokeAction : byte
        {
            BackToTitle,
            BackToDesktop,
        }

        private InvokeAction invokeAction;

        internal void InjectInvokeAction(InvokeAction invokeAction)
        {
            this.invokeAction = invokeAction;

            // 確認UIのラベル文字も更新する
            confirmLabelText.text = invokeAction switch
            {
                InvokeAction.BackToTitle => "ゲームを終了して\nタイトルに戻りますか？",
                InvokeAction.BackToDesktop => "ゲームを終了して\nデスクトップに戻りますか？",
                _ => throw new ArgumentOutOfRangeException(nameof(invokeAction), invokeAction, null),
            };
        }

        private protected sealed override void OnClickSucceeded()
        {
            if (invokeAction == InvokeAction.BackToTitle)
            {
                LoadManager.Instance.BeginLoad(Scene.Title);
            }
            else // invokeAction == InvokeAction.BackToDesktop
            {
                GameQuitter.InvokeQuit();
            }
        }
    }
}
