using UnityEngine;
using System.Collections;

namespace Gameplay {
    public class BalanceZone : MonoBehaviour
    {
     
        [SerializeField] private GameObject fallenTree;
        [SerializeField] private float treeRotationDuration = 2f;

        private ScreenFader fader;
        private const float MaxAngle = -30f;

        private void Start()
        {
            fader = FindObjectOfType<ScreenFader>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                StartCoroutine(Fall());
            }
        }
        
        private IEnumerator Fall()
        {
            var elapsed = 0f;
            
            fader.FadeOut();

            var startRotation = fallenTree.transform.localEulerAngles;
            var startZ = startRotation.z;

            while (elapsed < treeRotationDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / treeRotationDuration);
                var angle = Mathf.Sin(t * Mathf.PI * 0.5f) * MaxAngle;

                fallenTree.transform.localRotation = Quaternion.Euler(
                    startRotation.x,
                    startRotation.y,
                    startZ + angle
                );

                yield return null;
            }
        }
    }
}
