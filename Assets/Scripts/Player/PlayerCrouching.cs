using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Player))]
    public class PlayerCrouching: MonoBehaviour
    {
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float crouchTransitionSpeed = 10f;
        [SerializeField] private float crouchSpeedMultiplier = 0.5f;
        
        private Player player;
        PlayerInput playerInput;
        InputAction crouchAction;

        Vector3 initialCameraPosition;
        private float currentHeight;
        float standingHeight;

        private bool IsCrouching => standingHeight - currentHeight > 0.1f;
        
        void Awake()
        {
            player = GetComponent<Player>();
            playerInput = GetComponent<PlayerInput>();
            crouchAction = playerInput.actions["crouch"];
        }

        void Start()
        {
            initialCameraPosition = player.cameraTransform.localPosition;
            standingHeight = currentHeight = player.Height;
        }
        
        private void OnEnable() => player.OnBeforeMove += OnBeforeMove;
        private void OnDisable() => player.OnBeforeMove -= OnBeforeMove;

        void OnBeforeMove()
        {
            var isTryingToCrouch = crouchAction.ReadValue<float>() > 0;
            
            var heightTarget = isTryingToCrouch ? crouchHeight : standingHeight;

            if (IsCrouching && !isTryingToCrouch)
            {
                var castOrigin = transform.position + new Vector3(0, currentHeight / 2, 0);
                if (Physics.Raycast(castOrigin, Vector3.up, out RaycastHit hit, 0.2f))
                {
                    var distanceToCeiling = hit.point.y - castOrigin.y;
                    heightTarget = Mathf.Max(
                        currentHeight + distanceToCeiling - 0.1f,
                        crouchHeight
                    );
                }
            }

            if (!Mathf.Approximately(heightTarget, currentHeight))
            {
                var crouchDelta = Time.deltaTime * crouchTransitionSpeed;
                currentHeight = Mathf.Lerp(currentHeight, heightTarget, crouchDelta);
            
                var halfHeightDifference = new Vector3(0, (standingHeight - currentHeight) / 2, 0);
                var newCameraPosition = initialCameraPosition - halfHeightDifference;
            
                player.cameraTransform.localPosition = newCameraPosition;
                player.Height = heightTarget;
            }

            if (IsCrouching)
            {
                player.movementSpeedMultiplier *= crouchSpeedMultiplier;
            }
        }
    }
}