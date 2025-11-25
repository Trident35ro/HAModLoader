namespace HAModLoaderAPI
{
    public static class Log
    {
        public static void Info(string message)
        {
            UnityEngine.Debug.Log("[{LogManager.GetModNameForLog()}] " + message);
        }

        public static void Warning(string message)
        {
            UnityEngine.Debug.LogWarning("[{LogManager.GetModNameForLog()}] " + message);
        }

        public static void Error(string message)
        {
            UnityEngine.Debug.LogError("[{LogManager.GetModNameForLog()}] " + message);
        }
    }
}