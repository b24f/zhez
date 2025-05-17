using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerPositionManager: MonoBehaviour
    {
        [SerializeField] private Vector3 startPosition;

        // private void Awake()
        // {
        //     DontDestroyOnLoad(gameObject);
        // }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Debug.Log($"{scene.name} loaded, mode: {mode}");
            // transform.position = startPosition;
        }
    }
}