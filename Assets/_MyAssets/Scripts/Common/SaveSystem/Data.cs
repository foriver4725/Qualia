namespace MyScripts.Common.SaveSystem;

[Serializable]
internal sealed class Data
{
    public SingleData[] Slots;
}

// セーブデータ1スロット分
[Serializable]
internal sealed class SingleData
{
    public bool[] HasFoundSOSSigns;
    public Vector3 PlayerPosition;
}

internal static class Constants
{
    internal const string DataKey = "SaveSystem_Data_1";

    internal const int SlotCount = 3;
    internal const int SOSSignCount = 100;
}
