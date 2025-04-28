using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogues
{
    public class DialoguePanelComponent: MonoBehaviour
    {
        [SerializeField] private String text;
        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var label = root.Q<Label>("dialogue-text");
            label.text = text;
        }
    }
}