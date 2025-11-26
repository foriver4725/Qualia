namespace MyScripts.Common.SaveSystem;

internal static class SaveLoadInvoker
{
    private static Data CreateData()
    {
        Data data = new();

        // 初期値を代入
        data.HasFoundSOSSigns = new bool[Constants.SOSSignCount];
        data.HasFoundSOSSigns.AsSpan().Fill(false);
        data.PlayerPosition = Vector3.zero;

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
            return;
        }

        data = JsonUtility.FromJson<Data>(json);
    }
}
