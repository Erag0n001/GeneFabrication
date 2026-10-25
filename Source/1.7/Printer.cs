namespace GeneFabrication;

internal static class Printer {
    private const string Prefix = "[GF]> ";

    public static void Log(string message) {
        Verse.Log.Message(Prefix + message);
    }

    public static void Warn(string message) {
        Verse.Log.Warning(Prefix +message);
    }
    
    public static void Error(string message) {
        Verse.Log.Error(Prefix +message);
    }

    public static void ErrorOnce(string message, int hash) {
        Verse.Log.ErrorOnce(Prefix + message, hash);
    }
}