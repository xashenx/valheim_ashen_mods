using BepInEx.Logging;

namespace EpicTitles
{
    public class ETLogger {
        private static ManualLogSource Log;

        public ETLogger(ManualLogSource log) {
            Log = log;
        }
        public void initLogger(ManualLogSource log) {
            Log = log;
        }

        public static void logInfo(string text){
            Log.LogInfo(text);
        }

        public static void logError(string text){
            Log.LogError(text);
        }
    }
}