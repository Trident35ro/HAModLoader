using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HAModLoaderAPI;

public class ModManager : MonoBehaviour
{
    internal HAModLoaderAPI.HAModLoaderAPI API;
    static string logFile;
    public static string LogFilePath => logFile;

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
    logDir = $"/storage/emulated/0/Android/obb/{Application.identifier}/Logs";
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

        HAModLoaderAPI.Log.Info("[ModManager] Initializing before scene load...");
        GameObject go = new GameObject("ModManager");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<ModManager>().LoadMods();
        Log.LogFile = logFile;
        Log.GetLoadedMods = () => ModRegistry.LoadedMods;
        HAModLoaderAPI.Log.Info("[ModManager] Mod loader initialized successfully.");
    }

    void LoadMods()
    {
        HAModLoaderAPI.Log.Info("[ModManager] Loading mods...");
        API = new HAModLoaderAPI.HAModLoaderAPI();
        ModRegistry.APIInstance = API;
        string[] modPaths = GetPlatformModPaths();

        foreach (string path in modPaths)
        {
            try
            {
                HAModLoaderAPI.Log.Info($"[ModManager] Found DLL: {path}");
                Assembly asm = Assembly.Load(File.ReadAllBytes(path));

                // Notify LoadItems of this assembly so it can scan HAItem-derived classes in the mod DLL
                try
                {
                    LoadItems.ScanAssembly(asm);
                    HAModLoaderAPI.Log.Info($"[ModManager] ScanAssembly called for {Path.GetFileName(path)}");
                }
                catch (Exception exScan)
                {
                    HAModLoaderAPI.Log.Warning($"[ModManager] LoadItems.ScanAssembly failed for {path}: {exScan}");
                }

                var modType = asm.GetTypes().FirstOrDefault(t => typeof(HAMod).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                if (modType != null)
                {
                    var mod = (HAMod)Activator.CreateInstance(modType);
                    ModRegistry.RegisterMod(mod);
                    SafeInvoke(mod, "OnPreLoad", API);
                    HAModLoaderAPI.Log.Info($"[ModManager] Loaded mod: {modType.FullName}");
                }
                else HAModLoaderAPI.Log.Warning($"[ModManager] No HAMod implementation found in {asm.FullName}");
            }
            catch (Exception ex)
            {
                HAModLoaderAPI.Log.Error($"[ModManager] Failed to load {path}\n{ex}");
            }
        }

        HAModLoaderAPI.Log.Info("[ModManager] All mods preloaded.");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HAModLoaderAPI.Log.Info($"[ModManager] Scene loaded: {scene.name}");
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
    dir = $"/storage/emulated/0/Android/obb/{Application.identifier}/Mods";
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
                Log.Info($"[ModManager] {mod.GetType().Name}.{method} executed successfully.");
            }
            catch (Exception e)
            {
                Log.Error($"[ModManager] Error in {mod.GetType().Name}.{method}: {e}");
            }
        }
    }
}
