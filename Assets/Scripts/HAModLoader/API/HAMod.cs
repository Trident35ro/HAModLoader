using UnityEngine;

namespace HAModLoaderAPI
{
    public interface HAMod
    {
        public string ModName => GetType().Name;
        public abstract void OnModLoad();
        public abstract void OnEnterMenu();
        public abstract void OnEnterGame();
        public abstract void OnCreate(UnityEngine.GameObject obj);
        public abstract void Update();
        protected void LogInfo(string msg) => Log.Info($"[{ModName}] {msg}");
        protected void LogWarning(string msg) => Log.Warning($"[{ModName}] {msg}");
        protected void LogError(string msg) => Log.Error($"[{ModName}] {msg}");
    }
}