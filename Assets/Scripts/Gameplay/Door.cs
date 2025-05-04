using UnityEngine;

namespace Gameplay
{
    public class Door: MonoBehaviour
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private float openSpeed = 20f;
        
        private bool isInteracting;
        private bool isInZone;
        private float currentRotation;
        private const float MaxOpenAngle = 90f;

        private void Update()
        {
            isInteracting = isInZone && Input.GetKey(KeyCode.E);
            
            if (isInteracting)
            {
                RotateDoor();
            }
        }
        
        private void RotateDoor()
        {
           currentRotation += openSpeed * Time.deltaTime;
           
           currentRotation = Mathf.Clamp(currentRotation, 0, MaxOpenAngle);
           
           Quaternion targetRotation = Quaternion.Euler(0, currentRotation, 0);
           
           pivot.rotation = targetRotation;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isInZone = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isInZone = false;
            }
        }
    }
}