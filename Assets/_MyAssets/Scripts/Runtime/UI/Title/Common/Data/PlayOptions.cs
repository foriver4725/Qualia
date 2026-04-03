namespace MyScripts.Runtime.UI.Title;

// セーブスロット画面で選択されたプレイ設定を記録しておく (何のデータをどうプレイ開始するか)
// UIへのインタラクトなどがあったら、即座に更新するべき
internal static class PlayOptions
{
    internal static int SlotIndex { get; set; } = 0;
    internal static bool IsNewGame { get; set; } = true;
}