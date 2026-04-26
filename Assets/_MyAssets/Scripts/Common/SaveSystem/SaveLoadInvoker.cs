using System.IO;

namespace MyScripts.Common.SaveSystem;

internal static class SaveLoadInvoker
{
    /// <summary>
    /// <para>セーブ</para>
    /// <para>上書き保存, UTF8 (BOMなし)</para>
    /// <para>ディレクトリが存在しない場合は自動生成する</para>
    /// <para>エラーが起こったら処理を中断するが、例外は投げない</para>
    /// </summary>
    // TODO: 保存中にアプリ終了したりするとデータが破損するかもなので、テンポラリファイルに保存してから差し替えるようにしたい
    // TODO: 簡易でいいので暗号化したい
    internal static void Save(Data data)
    {
        try
        {
            string json = JsonUtility.ToJson(data);
            if (string.IsNullOrEmpty(json))
                throw new Exception("<JsonUtility.ToJson> resulted in null or empty string.");

            string saveDirectoryPath = SaveDirectoryPath;
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
    /// <para>ディレクトリが存在しない・ファイルが存在しない場合は、デフォルトのデータオブジェクトを新規作成して返す</para>
    /// <para>エラーが起こったら中断して、デフォルトのデータオブジェクトを新規作成して返す</para>
    /// </summary>
    // TODO: 簡易でいいので暗号化したい
    internal static void Load(out Data data)
    {
        try
        {
            // string saveDirectoryPath = Steam.CloudSavePathProvider.CreateDirectoryPath(Application.persistentDataPath);
            string saveDirectoryPath = SaveDirectoryPath;
            if (!Directory.Exists(saveDirectoryPath))
            {
                data = Data.CreateDefault();
                $"[{nameof(SaveLoadInvoker)}] No directory found. Created new data object.".Print();
                return;
            }

            string saveFilePath = Path.Combine(saveDirectoryPath, Constants.SaveFileName);
            if (!File.Exists(saveFilePath))
            {
                data = Data.CreateDefault();
                $"[{nameof(SaveLoadInvoker)}] No file found. Created new data object.".Print();
                return;
            }

            using (StreamReader sr = new(saveFilePath, Constants.SaveFileEncoding))
            {
                string json = sr.ReadToEnd();
                data = JsonUtility.FromJson<Data>(json);
                if (data == null)
                    throw new Exception("<JsonUtility.FromJson> resulted in null data object.");
            }

            $"[{nameof(SaveLoadInvoker)}] Load succeeded.".Print();
            return;
        }
        catch (Exception e)
        {
            data = Data.CreateDefault();
            $"[{nameof(SaveLoadInvoker)}] Load failed: {e}. Created new data object.".Print(LogSettings.Error);
            return;
        }
    }

    private static readonly string SaveDirectoryPath =
        Path.Combine(Application.persistentDataPath, "SaveData", "GameDungeon12", "TestUser");
}