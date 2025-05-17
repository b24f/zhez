using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public class Trigger: MonoBehaviour
    {
        [SerializeField] private bool destroyOnTriggerEnter;
        [SerializeField] private string tagFilter;
        [SerializeField] private UnityEvent onTriggerEnter;
        [SerializeField] private UnityEvent onTriggerExit;

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(tagFilter) && !other.gameObject.CompareTag(tagFilter)) return;
            onTriggerEnter?.Invoke();
            if (destroyOnTriggerEnter)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!string.IsNullOrEmpty(tagFilter) && !other.gameObject.CompareTag(tagFilter)) return;
            onTriggerExit?.Invoke();
        }
    }
}