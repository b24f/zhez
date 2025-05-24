using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Core;

namespace UIComponents
{
    public class ExamineComponent : MonoBehaviour
    {
        [SerializeField] protected UIDocument document;

        private Label page;

        private static IReadOnlyDictionary<string, string> PageDictionary { get; } = new Dictionary<string, string>
        {
            {
                "1", "No no"
            },
            {
                "2", "Yes yes"
            }
        };

        private void OnEnable()
        {
            EventEmitter.OnPageOpen += OnPageOpen;
        }

        private void OnDisable()
        {
            EventEmitter.OnPageOpen -= OnPageOpen;
        }

        private void OnPageOpen(string id)
        {
            page = document.rootVisualElement.Q<Label>("page");
            page.text = PageDictionary[id];
        }
    }
}