using System;
using UnityEngine;
using Core;

namespace UIComponents
{
    public class InteractionPrompt : MonoBehaviour
    {
        [SerializeField] private CanvasGroup promptCanvasGroup;
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float activationDistance = 2f;
        [SerializeField] private bool destroyAfterInteraction = false;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private string id;
        
        private bool isVisible;

        private void Start()
        {
            promptCanvasGroup.alpha = 0f;
        }

        private void Update()
        {
            var distance = Vector3.Distance(transform.position, playerTransform.position);
            var shouldShow = distance <= activationDistance;

            if (shouldShow != isVisible)
            {
                isVisible = shouldShow;
                StopAllCoroutines();
                StartCoroutine(FadeCanvasGroup(promptCanvasGroup, isVisible ? 1f : 0f));
            }
            
            if (isVisible && Input.GetKeyDown(KeyCode.E) && distance <= 1.5f)
            {
                EventEmitter.EmitStateChange(GameState.Current.state == Types.State.Examine ? Types.State.Play : Types.State.Examine);
                EventEmitter.EmitPageOpen(id);
            }
        }

        private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha)
        {
            var startAlpha = cg.alpha;
            var time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
                yield return null;
            }

            cg.alpha = targetAlpha;
        }
    }
}