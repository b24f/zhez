using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Player))]
    public class PlayerSprinting: MonoBehaviour
    {
        [SerializeField] private float speedMultiplier = 2f;
        
        private Player player;
        PlayerInput playerInput;
        InputAction sprintAction;

        private void Awake()
        {
            player = GetComponent<Player>();
            playerInput = GetComponent<PlayerInput>();
            sprintAction = playerInput.actions["sprint"];
        }

        private void OnEnable() => player.OnBeforeMove += OnBeforeMove;
        private void OnDisable() => player.OnBeforeMove -= OnBeforeMove;

        private void OnBeforeMove()
        {
            var sprintInput = sprintAction.ReadValue<float>();
            
            if (sprintInput == 0) return;
            
            var forwardMovementFactor = Mathf.Clamp01(
                Vector3.Dot(player.transform.forward, player.velocity.normalized)   
            );
            var multiplier = Mathf.Lerp(1f, speedMultiplier, forwardMovementFactor);
            
            player.MovementSpeedMultiplier *= multiplier;
        }
    }
}