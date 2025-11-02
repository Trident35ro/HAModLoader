using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HAModLoaderAPI;

    public class ModManager : MonoBehaviour
{
        public HAModLoaderAPI.HAModLoaderAPI API;
        static string logFile;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            string logDir;
#if UNITY_ANDROID
        logDir = Path.Combine(Application.persistentDataPath, "Logs");
#elif UNITY_STANDALONE_WIN
        logDir = Path.Combine(Application.dataPath, "Logs");
#else
            logDir = Path.Combine(Application.streamingAssetsPath, "Logs");
#endif
#if UNITY_ANDROID
    logDir = $"/storage/emulated/0/Android/media/{Application.identifier}/Logs";
#elif UNITY_STANDALONE_WIN
    logDir = Path.Combine(Application.dataPath, "../Logs");
#elif UNITY_STANDALONE_LINUX
    logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "../.local/share/HAModLoader/Logs");
#elif UNITY_STANDALONE_OSX
    logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "../Library/Application Support/HAModLoader/Logs");
#else
    logDir = Path.Combine(Application.streamingAssetsPath, "Logs");
#endif

        if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            logFile = Path.Combine(logDir, $"ModLoader_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            File.WriteAllText(logFile, $"[ModManager] Log started at {DateTime.Now}\n");

            Debug.Log("[ModManager] Initializing before scene load...");
            GameObject go = new GameObject("ModManager");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<ModManager>().LoadMods();

            Debug.Log("[ModManager] Mod loader initialized successfully.");
        }

        static void Log(string msg)
        {
            Debug.Log(msg);
            try { File.AppendAllText(logFile, msg + "\n"); } catch { }
        }

        static void LogError(string msg)
        {
            Debug.LogError(msg);
            try { File.AppendAllText(logFile, "[ERROR] " + msg + "\n"); } catch { }
        }

        static void LogWarning(string msg)
        {
            Debug.LogWarning(msg);
            try { File.AppendAllText(logFile, "[WARN] " + msg + "\n"); } catch { }
        }

        void LoadMods()
        {
            Log("[ModManager] Loading mods...");
            API = new HAModLoaderAPI.HAModLoaderAPI();
            string[] modPaths = GetPlatformModPaths();

            foreach (string path in modPaths)
            {
                try
                {
                    Log($"[ModManager] Found DLL: {path}");
                    Assembly asm = Assembly.Load(File.ReadAllBytes(path));
                    var modType = asm.GetTypes().FirstOrDefault(t => typeof(HAMod).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    if (modType != null)
                    {
                        HAMod mod = (HAMod)Activator.CreateInstance(modType);
                        API.RegisterMod(mod);
                        SafeInvoke(mod, "OnPreLoad", API);
                        Log($"[ModManager] Loaded mod: {modType.FullName}");
                    }
                    else LogWarning($"[ModManager] No HAMod implementation found in {asm.FullName}");
                }
                catch (Exception ex)
                {
                    LogError($"[ModManager] Failed to load {path}\n{ex}");
                }
            }

            Log("[ModManager] All mods preloaded.");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log($"[ModManager] Scene loaded: {scene.name}");
            if (scene.name == "Menu")
                foreach (var mod in API.loadedMods) SafeInvoke(mod, "OnEnterMenu");
            else if (scene.name == "Game")
                foreach (var mod in API.loadedMods) SafeInvoke(mod, "OnEnterGame");
        }

        void Update()
        {
            foreach (var mod in API.loadedMods)
                SafeInvoke(mod, "Update");
        }

        static string[] GetPlatformModPaths()
        {
            string dir;
#if UNITY_ANDROID
    dir = $"/storage/emulated/0/Android/media/{Application.identifier}/Mods";
#elif UNITY_STANDALONE_WIN
    dir = Path.Combine(Application.dataPath, "../Mods");
#elif UNITY_STANDALONE_LINUX
    dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "../.local/share/HAModLoader/Mods");
#elif UNITY_STANDALONE_OSX
    dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "../Library/Application Support/HAModLoader/Mods");
#else
    dir = Path.Combine(Application.streamingAssetsPath, "Mods");
#endif
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Directory.GetFiles(dir, "*.dll");
        }

        void SafeInvoke(HAMod mod, string method, params object[] args)
        {
            var m = mod.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (m != null)
            {
                try
                {
                    m.Invoke(mod, args);
                    Log($"[ModManager] {mod.GetType().Name}.{method} executed successfully.");
                }
                catch (Exception e)
                {
                    LogError($"[ModManager] Error in {mod.GetType().Name}.{method}: {e}");
                }
            }
        }
    }