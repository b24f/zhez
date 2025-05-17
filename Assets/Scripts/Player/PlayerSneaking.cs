using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Player))]
    public class PlayerSneaking: MonoBehaviour
    {
        [SerializeField] private float speedMultiplier = 0.5f;

        private Player player;
        PlayerInput playerInput;
        private InputAction sneakInput;
        
        private void Awake()
        {
            player = GetComponent<Player>();
            playerInput = GetComponent<PlayerInput>();
            sneakInput = playerInput.actions["sneak"];
        }

        private void OnEnable() => player.OnBeforeMove += OnBeforeMove;
        private void OnDisable() => player.OnBeforeMove -= OnBeforeMove;

        private void OnBeforeMove()
        {
            var isSneaking = sneakInput.ReadValue<float>() > 0;

            if (isSneaking)
            {
                player.MovementSpeedMultiplier *= speedMultiplier;
            }
        }
    }
}