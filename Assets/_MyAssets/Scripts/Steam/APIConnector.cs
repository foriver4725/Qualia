// using Steamworks;
//
// namespace MyScripts.Steam
// {
//     public static class APIConnector
//     {
//         internal static bool HasInitialized { get; private set; } = false;
//
//         /// <summary>
//         /// <para>SteamAPI を初期化する</para>
//         /// <para>成功したら true, 失敗したら false を返す</para>
//         /// <para>成功したら、以降の呼び出しは全て無視され、必ず true を返す</para>
//         /// </summary>
//         public static bool Init()
//         {
//             if (HasInitialized)
//                 return true;
//
//             bool result = SteamAPI.Init();
//             if (result)
//                 HasInitialized = true;
//
//             return result;
//         }
//     }
// }
