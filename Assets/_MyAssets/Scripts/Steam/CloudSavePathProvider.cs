// using System.IO;
// using Steamworks;
//
// namespace MyScripts.Steam
// {
//     public static class CloudSavePathProvider
//     {
//         /// <summary>
//         /// <para>Steam Cloud のセーブデータ保存先ディレクトリパスを生成する</para>
//         /// <para>現在のユーザーIDごとにディレクトリが分かれるようになっている</para>
//         /// </summary>
//         /// <param name="persistentDataPath">Application.persistentDataPath = %UserProfile%/AppData/LocalLow/[CompanyName]/[ProductName]</param>
//         /// <returns>%UserProfile%/AppData/LocalLow/[CompanyName]/[ProductName]/SaveData/Steam/[SteamUserID]</returns>
//         public static string CreateDirectoryPath(string persistentDataPath)
//         {
//             // 64bit
//             ulong steamUserId = SteamUser.GetSteamID().m_SteamID;
//             return Path.Combine(persistentDataPath, "SaveData", "Steam", steamUserId.ToString());
//         }
//     }
// }
