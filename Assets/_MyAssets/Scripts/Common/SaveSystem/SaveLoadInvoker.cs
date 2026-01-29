using System.IO;
using System.Text;

namespace MyScripts.Common.SaveSystem;

internal static class SaveLoadInvoker
{
    internal static SingleData CreateDefaultSingleData()
    {
        SingleData slot = new();

        // 初期値を代入
        {
            slot.IsValid = false;

            slot.HasObtainedAnima = false;

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
    /// <para>セーブ</para>
    /// <para>上書き保存, UTF8 (BOMなし)</para>
    /// <para>ディレクトリが存在しない場合は自動生成する</para>
    /// <para>エラーが起こったら処理を中断するが、例外は投げない</para>
    /// </summary>
    // TODO: 保存中にアプリ終了したりするとデータが破損するかもなので、テンポラリファイルに保存してから差し替えるようにしたい
    internal static void Save(Data data)
    {
        try
        {
            string json = JsonUtility.ToJson(data);
            if (string.IsNullOrEmpty(json))
                throw new Exception("<JsonUtility.ToJson> resulted in null or empty string.");

            string saveDirectoryPath = Steam.CloudSavePathProvider.CreateDirectoryPath(Application.persistentDataPath);
            if (!Directory.Exists(saveDirectoryPath))
                Directory.CreateDirectory(saveDirectoryPath);

            string saveFilePath = Path.Combine(saveDirectoryPath, Constants.SaveFileName);
            using (StreamWriter sw = new(saveFilePath, false, Constants.SaveFileEncoding))
            {
                sw.Write(json.AsSpan());
            }

            $"[{nameof(SaveLoadInvoker)}] Save succeeded.".Print();
            return;
        }
        catch (Exception e)
        {
            $"[{nameof(SaveLoadInvoker)}] Save failed: {e}".Print(LogSettings.Error);
            return;
        }
    }

    /// <summary>
    /// <para>ロード</para>
    /// <para>UTF8 (BOMなし)</para>
    /// <para>ディレクトリが存在しない・ファイルが存在しない場合は、データのデフォルトインスタンスを新規作成して返す</para>
    /// <para>エラーが起こったら中断して、データのデフォルトインスタンスを新規作成して返す</para>
    /// </summary>
    internal static void Load(out Data data)
    {
        try
        {
            string saveDirectoryPath = Steam.CloudSavePathProvider.CreateDirectoryPath(Application.persistentDataPath);
            if (!Directory.Exists(saveDirectoryPath))
            {
                data = CreateDefaultData();
                $"[{nameof(SaveLoadInvoker)}] No directory found. Created new data.".Print();
                return;
            }

            string saveFilePath = Path.Combine(saveDirectoryPath, Constants.SaveFileName);
            if (!File.Exists(saveFilePath))
            {
                data = CreateDefaultData();
                $"[{nameof(SaveLoadInvoker)}] No file found. Created new data.".Print();
                return;
            }

            using (StreamReader sr = new(saveFilePath, Constants.SaveFileEncoding))
            {
                string json = sr.ReadToEnd();
                data = JsonUtility.FromJson<Data>(json);
                if (data == null)
                    throw new Exception("<JsonUtility.FromJson> resulted in null data instance.");
            }

            $"[{nameof(SaveLoadInvoker)}] Load succeeded.".Print();
            return;
        }
        catch (Exception e)
        {
            data = CreateDefaultData();
            $"[{nameof(SaveLoadInvoker)}] Load failed: {e}. Created new data.".Print(LogSettings.Error);
            return;
        }
    }
}
