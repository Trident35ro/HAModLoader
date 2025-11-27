using UnityEngine;
using UnityEngine.SceneManagement;

namespace HAModLoaderAPI
{
    public interface HAMod
    {
        public string ModName => GetType().Name;
        public string ModAuthor => GetType().Name;

        public void OnModLoad() { }
        public void OnEnterScene(Scene scene) { }
        public void OnCreate(GameObject obj) { }
        public void Update() { }
    }
}