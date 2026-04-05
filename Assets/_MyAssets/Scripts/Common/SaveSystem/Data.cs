namespace MyScripts.Common.SaveSystem;

[Serializable]
internal sealed class Data
{
    public SingleData[] Slots;

    public static Data CreateDefault()
    {
        Data data = new();

        data.Slots = new SingleData[Constants.SlotCount];
        for (int i = 0; i < data.Slots.Length; i++)
        {
            data.Slots[i] = SingleData.CreateDefault();
        }

        return data;
    }
}

// セーブデータ1スロット分
[Serializable]
internal sealed class SingleData
{
    public bool IsValid;
    public int SOSAnimaArrangementSeed;
    public bool HasObtainedAnima;
    public string LastSavedAt;
    public string LastScreenshotSavedPath;
    public bool[] HasFoundSOSSigns;
    public Vector3 PlayerPosition;
    public Vector3 PlayerForward;

    public static readonly Vector3 PlayerPositionDefault = new(-96, -21, 110);

    public static readonly Vector3 PlayerForwardDefault
        = new Vector3(-Mathf.Sin(30 * Mathf.Deg2Rad), 0, Mathf.Cos(30 * Mathf.Deg2Rad)).normalized;

    public static SingleData CreateDefault()
    {
        SingleData slot = new();

        slot.IsValid = false;
        slot.SOSAnimaArrangementSeed = 0;
        slot.HasObtainedAnima = false;
        slot.SetLastSavedAt(DateTime.MinValue);
        slot.LastScreenshotSavedPath = "";
        slot.HasFoundSOSSigns = new bool[Constants.SOSSignCount];
        slot.HasFoundSOSSigns.AsSpan().Fill(false);
        // 汚いけど、ここで初期位置を決め打ちしてしまう
        slot.PlayerPosition = PlayerPositionDefault;
        slot.PlayerForward = PlayerForwardDefault;

        return slot;
    }

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
    // 拡張子付きファイル名
    internal const string SaveFileName = "SaveData_1.dat";

    // UTF8 (BOMなし)
    internal static readonly System.Text.Encoding SaveFileEncoding = new System.Text.UTF8Encoding(false);

    internal const int SlotCount = 3;
    internal const int SOSSignCount = 100;
}

// 外部から読み書きする
// セーブデータには含まれないワールドのプレイ情報を受け渡すために、特別処理
internal static class Variables
{
    internal static int CurrentSlotIndex { get; set; } = 0;
    internal static bool IsFirstPlay { get; set; } = false;
}