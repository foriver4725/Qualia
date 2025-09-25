namespace MyScripts.Runtime;

internal interface IDisasterManager
{
    Disaster MyType { get; }

    /// <summary>
    /// ! 複数インスタンスでテキストを使いまわすため、全て false にしてから新規で true にすること
    /// </summary>
    bool Enabled { get; set; }
}
