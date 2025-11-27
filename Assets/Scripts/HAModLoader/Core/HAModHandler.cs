using UnityEngine;
using UnityEngine.SceneManagement;
using HAModLoaderAPI;

public class HAModHandler
{
    private HAModLoaderAPI.HAModLoaderAPI API;

    public void Initialize(HAModLoaderAPI.HAModLoaderAPI api)
    {
        API = api;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var mod in API.loadedMods)
            SafeInvoke(mod, "OnEnterScene", scene);
    }

    void Update()
    {
        if (API == null) return;

        foreach (var mod in API.loadedMods)
            SafeInvoke(mod, "Update");
    }

    public void SafeInvoke(HAMod mod, string method, params object[] args)
    {
        var m = mod.GetType().GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (m == null) return;

        try
        {
            m.Invoke(mod, args);
        }
        catch (System.Exception e)
        {
            Log.Error($"[ModLifecycleHandler] Error in {mod.GetType().Name}.{method}: {e}");
        }
    }
}
