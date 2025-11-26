namespace MyScripts.Common.SaveSystem;

internal static class SaveLoadInvoker
{
    private static Data CreateData()
    {
        Data data = new();

        // 初期値を代入
        data.Slots = new SingleData[Constants.SlotCount];
        for (int i = 0; i < Constants.SlotCount; i++)
        {
            SingleData slot = new();
            {
                slot.HasFoundSOSSigns = new bool[Constants.SOSSignCount];
                slot.HasFoundSOSSigns.AsSpan().Fill(false);
                slot.PlayerPosition = Vector3.zero;
            }
            data.Slots[i] = slot;
        }

        return data;
    }

    /// <summary>
    /// セーブ<br/>
    /// 既存データの上書き保存
    /// </summary>
    internal static void Save(Data data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(Constants.DataKey, json);

        $"[{nameof(SaveLoadInvoker)}] Data saved...\n{json}".Print();
    }

    /// <summary>
    /// ロード<br/>
    /// データが存在しない場合は新規作成して返す
    /// </summary>
    /// <param name="data"></param>
    internal static void Load(out Data data)
    {
        string json = PlayerPrefs.GetString(Constants.DataKey, string.Empty);

        if (string.IsNullOrEmpty(json))
        {
            data = CreateData();

            $"[{nameof(SaveLoadInvoker)}] No data found. Created new data.".Print();

            return;
        }

        data = JsonUtility.FromJson<Data>(json);

        $"[{nameof(SaveLoadInvoker)}] Data loaded...\n{json}".Print();
    }
}
