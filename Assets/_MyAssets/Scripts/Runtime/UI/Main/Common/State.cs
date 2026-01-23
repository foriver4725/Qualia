namespace MyScripts.Runtime.UI.Main;

/// <summary>
/// 最前面にあるUIを表現
/// </summary>
internal enum State : byte
{
    // デフォルト
    Default, // -> Pause

    // ポーズ中
    Pause, // -> Default, Back_Confirm

    // ゲームを終了して戻る 確認
    Back_Confirm, // -> Pause, [Back]
}
