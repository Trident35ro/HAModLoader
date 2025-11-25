using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HAModLoaderAPI
{
    public static class Log
    {
        private static string prefix;
        public static string LogFile;
        public static Func<HAMod[]> GetLoadedMods;

        private static string GetPrefix()
        {
            var stack = new StackTrace();
            foreach (var frame in stack.GetFrames())
            {
                var type = frame.GetMethod()?.DeclaringType;
                if (type == null) continue;

                if (typeof(HAMod).IsAssignableFrom(type))
                {
                    // Use the delegate to get mods, avoids direct dependency on ModRegistry
                    var mods = GetLoadedMods?.Invoke();
                    var mod = mods?.FirstOrDefault(m => m.GetType() == type);
                    if (mod != null)
                        return $"[{mod.ModName}]";
                }
            }
            return "[HAMod]";
        }

        public static void Info(string message)
        {
            UnityEngine.Debug.Log($"{GetPrefix()} {message}");
            try { File.AppendAllText(LogFile, $"{GetPrefix()} {message}\n"); } catch { }
        }

        public static void Warning(string message)
        {
            UnityEngine.Debug.LogWarning($"{GetPrefix()} {message}");
            try { File.AppendAllText(LogFile, $"{GetPrefix()} [WARN] {message}\n"); } catch { }
        }

        public static void Error(string message)
        {
            UnityEngine.Debug.LogError($"{GetPrefix()} {message}");
            try { File.AppendAllText(LogFile, $"{GetPrefix()} [ERROR] {message}\n"); } catch { }
        }
    }
}
