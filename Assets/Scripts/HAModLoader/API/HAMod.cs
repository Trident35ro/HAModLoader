using UnityEngine;

namespace HAModLoaderAPI
{
    public interface HAMod
    {
        public string ModName => GetType().Name;
        public string ModAuthor => GetType().Name;
        public abstract void OnModLoad();
        public abstract void OnEnterMenu();
        public abstract void OnEnterGame();
        public abstract void OnCreate(UnityEngine.GameObject obj);
        public abstract void Update();
    }
}