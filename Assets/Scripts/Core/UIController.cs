using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Current;

    [SerializeField] private GameObject HUDComponent;
    [SerializeField] private GameObject MenuComponent;
    [SerializeField] private GameObject DialoguePanelComponent;
   
    private void Awake()
    {
        Current = this;
    }

    public void Reset()
    {
        MenuComponent.SetActive(false);
        DialoguePanelComponent.SetActive(false);
    }

    public void RenderMenu()
    {
        Reset();
        MenuComponent.SetActive(true);
    }

    public void RenderDialogue()
    {
        Reset();
        DialoguePanelComponent.SetActive(true);
    }
}
