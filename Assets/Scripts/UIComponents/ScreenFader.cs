using UnityEngine;
using UnityEngine.UIElements;

public class ScreenFader : MonoBehaviour
{
    private VisualElement fadeOverlay;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        fadeOverlay = root.Q<VisualElement>("fade-overlay");
    }

    public void FadeOut()
    {
        fadeOverlay.style.opacity = 1f;
    }

    public void FadeIn()
    {
        fadeOverlay.style.opacity = 0f;
    }
}