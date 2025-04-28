using UnityEngine;

namespace Dialogues
{
    public class DialogueTrigger: MonoBehaviour
    {
        [SerializeField] private GameObject DialoguePanelComponent;
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                DialoguePanelComponent.SetActive(true);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                DialoguePanelComponent.SetActive(false);
            }
        }
    }
}