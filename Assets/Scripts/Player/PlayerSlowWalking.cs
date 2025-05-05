using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Player))]
    public class PlayerSlowWalking: MonoBehaviour
    {
        [SerializeField] private float speedMultiplier = 0.5f;

        private Player player;
        PlayerInput playerInput;
        private InputAction slowWalkAction;
        
        private void Awake()
        {
            player = GetComponent<Player>();
            playerInput = GetComponent<PlayerInput>();
            slowWalkAction = playerInput.actions["walk"];
        }

        private void OnEnable() => player.OnBeforeMove += OnBeforeMove;
        private void OnDisable() => player.OnBeforeMove -= OnBeforeMove;

        private void OnBeforeMove()
        {
            var isSlowWalking = slowWalkAction.ReadValue<float>() > 0;

            if (isSlowWalking)
            {
                player.MovementSpeedMultiplier *= speedMultiplier;
            }
        }
    }
}