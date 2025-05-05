using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Player))]
    public class PlayerPeeking : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float maxPeekAngle = 35f;
        [SerializeField] private float peekSpeed = 5f;

        private Player player;
        private PlayerInput playerInput;
        private InputAction peekAction;

        private float currentZRotation;

        private void Awake()
        {
            player = GetComponent<Player>();
            playerInput = GetComponent<PlayerInput>();
            peekAction = playerInput.actions["peek"];
        }

        private void OnEnable() => player.OnBeforeMove += OnBeforeMove;
        private void OnDisable() => player.OnBeforeMove -= OnBeforeMove;

        private void OnBeforeMove()
        {
            var peekInput = peekAction.ReadValue<float>();
            var isPeeking = Mathf.Abs(peekInput) > 0.01f;
            
            var targetAngle = isPeeking ? peekInput * maxPeekAngle : 0f;

            currentZRotation = Mathf.Lerp(currentZRotation, targetAngle, Time.deltaTime * peekSpeed);
            cameraPivot.localRotation = Quaternion.Euler(0f, 0f, currentZRotation);
        }
    }
}