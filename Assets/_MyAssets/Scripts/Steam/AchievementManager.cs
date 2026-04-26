// using Steamworks;
//
// namespace MyScripts.Steam
// {
//     public static class AchievementManager
//     {
//         /// <summary>
//         /// 達成型アチーブメント
//         /// </summary>
//         /// <returns>成功したら true, 失敗したら false</returns>
//         public static bool GetBool(string name, out bool value)
//         {
//             value = default;
//
//             // 未初期化
//             if (!APIConnector.HasInitialized) return false;
//
//             return SteamUserStats.GetAchievement(name, out value);
//         }
//
//         /// <summary>
//         /// 進捗型アチーブメント (整数)
//         /// </summary>
//         /// <returns>成功したら true, 失敗したら false</returns>
//         public static bool GetInt(string name, out int value)
//         {
//             value = default;
//
//             // 未初期化
//             if (!APIConnector.HasInitialized) return false;
//
//             return SteamUserStats.GetStat(name, out value);
//         }
//
//         /// <summary>
//         /// 進捗型アチーブメント (実数)
//         /// </summary>
//         /// <returns>成功したら true, 失敗したら false</returns>
//         public static bool GetFloat(string name, out float value)
//         {
//             value = default;
//
//             // 未初期化
//             if (!APIConnector.HasInitialized) return false;
//
//             return SteamUserStats.GetStat(name, out value);
//         }
//
//         /// <summary>
//         /// 達成型アチーブメント (true へのみセット可能! 他メソッドとの一貫性を保つため、このようなシグネチャにしている)
//         /// </summary>
//         /// <returns>成功したら true, 失敗したら false</returns>
//         public static bool SetBool(string name, bool value)
//         {
//             // 未初期化
//             if (!APIConnector.HasInitialized) return false;
//
//             // false から true へのセットのみ有効
//             if (!value) return false;
//             if (!GetBool(name, out bool currentValue)) return false;
//             if (currentValue) return false;
//
//             if (!SteamUserStats.SetAchievement(name)) return false;
//             if (!SteamUserStats.StoreStats()) return false;
//
//             return true;
//         }
//
//         /// <summary>
//         /// 進捗型アチーブメント (整数)
//         /// </summary>
//         /// <returns>成功したら true, 失敗したら false</returns>
//         public static bool SetInt(string name, int value)
//         {
//             // 未初期化
//             if (!APIConnector.HasInitialized) return false;
//
//             if (!SteamUserStats.SetStat(name, value)) return false;
//             if (!SteamUserStats.StoreStats()) return false;
//
//             return true;
//         }
//
//         /// <summary>
//         /// 進捗型アチーブメント (実数)
//         /// </summary>
//         /// <returns>成功したら true, 失敗したら false</returns>
//         public static bool SetFloat(string name, float value)
//         {
//             // 未初期化
//             if (!APIConnector.HasInitialized) return false;
//
//             if (!SteamUserStats.SetStat(name, value)) return false;
//             if (!SteamUserStats.StoreStats()) return false;
//
//             return true;
//         }
//     }
// }
