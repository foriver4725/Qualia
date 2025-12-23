namespace MyScripts.Runtime.UI.Title;

/// <summary>
/// 最前面にあるUIを表現
/// </summary>
internal enum State : byte
{
    // デフォルト
    Default, // -> Quit_Confirm, SaveSlot_Select

    // ゲーム終了 確認
    Quit_Confirm, // -> None, [Quit]

    // セーブスロット
    SaveSlot_Select, // -> None, SaveSlot_Select_PlayOption
    SaveSlot_Select_PlayOption, // -> SaveSlot_Select, SaveSlot_FinalConfirm
    SaveSlot_FinalConfirm, // -> SaveSlot_Select_PlayOption, [StartGame]

    // ロード中の暗転
    HidingAll, // -> .
}
