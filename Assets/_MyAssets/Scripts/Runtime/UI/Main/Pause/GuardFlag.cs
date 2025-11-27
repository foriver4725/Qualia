namespace MyScripts.Runtime.UI.Main.Pause;

/// <summary>
/// 「重要なボタン」とは、クリックした時に非同期かつ不可逆的な操作を行うボタンとする<br/>
/// (例えば「他シーンに遷移」などのボタンが該当する)<br/>
/// <br/>
/// このフラグは、任意の重要なボタンがクリックされた時、その他の重要なボタンを無効にするためのもの<br/>
/// このUIでのみ使用 (読み書きされる) 想定<br/>
/// </summary>
internal static class GuardFlag
{
    // NOTE: static なので、Awake()でfalseに戻してほしい
    internal static bool IsLocked { get; set; } = false;
}
