namespace MyScripts.Runtime.UI.Main;

internal enum BackType : byte
{
    BackToTitle,
    BackToDesktop,
}

// タイトルに戻る方法を記録して、判別のもとにする
// UIへのインタラクトなどがあったら、即座に更新するべき
internal static class BackOptions
{
    internal static BackType ChosenBackType { get; set; } = BackType.BackToTitle;
}

// ゲームプレイに関する情報を管理する
internal static class PlayInfo
{
    internal static bool IsFirstPlay { get; set; } = true;
}
