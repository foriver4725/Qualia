namespace MyScripts.Common.SaveSystem;

[Serializable]
internal struct Data
{
    internal bool[] HasFoundSOSSigns;
    internal Vector3 PlayerPosition;
}

internal static class Constants
{
    internal const string DataKey = "SaveSystem_Data_1";
    internal const int SOSSignCount = 100;
}
