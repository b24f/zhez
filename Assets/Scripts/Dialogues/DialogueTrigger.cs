using UnityEngine;
using System.Collections;

namespace Dialogues
{
    public class DialogueTrigger: MonoBehaviour
    {
        [SerializeField] private GameObject DialoguePanelComponent;
        [SerializeField] private string dialogue;
        
        private Coroutine coroutine;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                DialoguePanelComponent.SetActive(true);
                Dialogues.DialoguePanelComponent.Current.Show(dialogue);

                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }

                var duration = dialogue.Length * 0.1f;
                coroutine = StartCoroutine(HideDialogue(duration));
            }
        }

        // private void OnTriggerExit(Collider other)
        // {
        //     if (other.CompareTag("Player"))
        //     {
        //         DialoguePanelComponent.SetActive(false);
        //     }
        // }

        private IEnumerator HideDialogue(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            DialoguePanelComponent.SetActive(false);
            
            Destroy(gameObject);
        }
    }
}