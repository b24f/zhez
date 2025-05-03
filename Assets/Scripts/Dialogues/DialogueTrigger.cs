using UnityEngine;
using System;
using System.Timers;

namespace Dialogues
{
    public class DialogueTrigger: MonoBehaviour
    {
        [SerializeField] private GameObject DialoguePanelComponent;
        [SerializeField] private String dialogue;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                DialoguePanelComponent.SetActive(true);
                Dialogues.DialoguePanelComponent.Current.Show(dialogue);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                DialoguePanelComponent.SetActive(false);
            }
        }
    }
}