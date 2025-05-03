using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogues
{
    public class DialoguePanelComponent: MonoBehaviour
    {
        public static DialoguePanelComponent Current;

        private Label label;
        
        private void Awake()
        {
            Current = this;
        }
        
        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            label = root.Q<Label>("dialogue-text");
        }

        public void Show(String text)
        {
            label.text = text;
        }
    }
}