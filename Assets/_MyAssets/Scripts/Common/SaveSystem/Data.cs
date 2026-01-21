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
    public bool IsValid;
    public bool HasObtainedAnima;
    public string LastSavedAt;
    public string LastScreenshotSavedPath;
    public bool[] HasFoundSOSSigns;
    public Vector3 PlayerPosition;
    public Vector3 PlayerForward;

    public void SetLastSavedAt(DateTime dateTime)
    {
        LastSavedAt = dateTime.ToString();
    }

    public DateTime GetLastSavedAt()
    {
        if (DateTime.TryParse(LastSavedAt, out var result))
            return result;
        else
            return DateTime.MinValue;
    }
}

internal static class Constants
{
    internal const string DataKey = "SaveSystem_Data_1";

    internal const int SlotCount = 3;
    internal const int SOSSignCount = 100;
}

// 外部から読み書きする
internal static class Variables
{
    internal static int CurrentSlotIndex { get; set; } = 0;
}
