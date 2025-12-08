namespace MyScripts.Common.SaveSystem;

internal static class SaveLoadInvoker
{
    internal static SingleData CreateDefaultSingleData()
    {
        SingleData slot = new();

        // 初期値を代入
        {
            slot.IsValid = false;

            slot.SetLastSavedAt(DateTime.MinValue);
            slot.LastScreenshotSavedPath = "";

            slot.HasFoundSOSSigns = new bool[Constants.SOSSignCount];
            slot.HasFoundSOSSigns.AsSpan().Fill(false);

            // 汚いけど、ここで初期位置を決め打ちしてしまう
            slot.PlayerPosition = new Vector3(-96, -21, 110);
            slot.PlayerForward = new Vector3(-Mathf.Sin(30 * Mathf.Deg2Rad), 0, Mathf.Cos(30 * Mathf.Deg2Rad)).normalized;
        }

        return slot;
    }

    internal static Data CreateDefaultData()
    {
        Data data = new();

        // 初期値を代入
        {
            data.Slots = new SingleData[Constants.SlotCount];
            for (int i = 0; i < Constants.SlotCount; i++)
            {
                data.Slots[i] = CreateDefaultSingleData();
            }
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
            data = CreateDefaultData();

            $"[{nameof(SaveLoadInvoker)}] No data found. Created new data.".Print();

            return;
        }

        data = JsonUtility.FromJson<Data>(json);

        $"[{nameof(SaveLoadInvoker)}] Data loaded...\n{json}".Print();
    }
}
