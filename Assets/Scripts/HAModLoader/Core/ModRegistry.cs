using UnityEngine;
using System.IO;
using System;
using System.Linq;
using HAModLoaderAPI;

internal static class ModRegistry
{
    internal static HAModLoaderAPI.HAModLoaderAPI APIInstance;

    internal static HAMod[] LoadedMods => APIInstance?.loadedMods.ToArray() ?? Array.Empty<HAMod>();

    internal static void RegisterMod(HAMod mod)
    {
        if (APIInstance == null)
        {
            HAModLoaderAPI.Log.Error("[ModRegistry] Cannot register mod: APIInstance is null");
            return;
        }

        APIInstance.RegisterMod(mod);
        HAModLoaderAPI.Log.Info($"[ModRegistry] Registered mod: {mod.GetType().Name}");
    }
}