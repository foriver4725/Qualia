namespace MyScripts.Common;

internal static class MyLogger
{
    private const string SYMBOL = "MY_LOGGER";

    [Conditional(SYMBOL)] internal static void Log(this object message) => Debug.Log(message);
    [Conditional(SYMBOL)] internal static void LogWarning(this object message) => Debug.LogWarning(message);
    [Conditional(SYMBOL)] internal static void LogError(this object message) => Debug.LogError(message);
    [Conditional(SYMBOL)] internal static void LogException(this Exception exception) => Debug.LogException(exception);
}
