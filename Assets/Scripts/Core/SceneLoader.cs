using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Core
{
    public class SceneLoader: MonoBehaviour
    {
        public static SceneLoader Current;
        private void Awake()
        {
            Current = this;
        }
        
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        public void StartAsyncLoad(string sceneName)
        {
            StartCoroutine(LoadAsyncScene(sceneName));
        }

        private IEnumerator LoadAsyncScene(string sceneName)
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            
            while (!async.isDone)
            {
                yield return null;
            }
        }
    }
}