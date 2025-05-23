using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private CanvasGroup promptCanvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float activationDistance = 2f;
    [SerializeField] private bool destroyAfterInteraction = false;
    [SerializeField] private GameObject journalPage;

    private Transform player;
    private bool isVisible = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        promptCanvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool shouldShow = distance <= activationDistance;

        if (shouldShow != isVisible)
        {
            isVisible = shouldShow;
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(promptCanvasGroup, isVisible ? 1f : 0f));
        }
        
        if (isVisible && Input.GetKeyDown(KeyCode.E) && distance <= 1.5f)
        {
            journalPage.SetActive(true);
            Time.timeScale = 0;
        }

        if (journalPage.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1f;
            journalPage.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha)
    {
        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        cg.alpha = targetAlpha;
    }
}