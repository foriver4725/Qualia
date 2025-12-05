namespace MyScripts.Runtime.UI.Title;

// ゲームスタートの設定を記録しておく
// UIへのインタラクトなどがあったら、即座に更新するべき
internal static class StartSettings
{
    internal static int SlotIndex { get; set; } = 0;
    internal static bool IsNewGame { get; set; } = true;
}
