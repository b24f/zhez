using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Player
{
    public class Player: MonoBehaviour
    {
        public Transform cameraTransform;
        
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private float movementSpeed = 4f;
        [SerializeField] private float mass = 1f;
        [SerializeField] private float acceleration = 20f;
        
        // Peeking start
        [SerializeField] private Transform cameraPivot;
        private float maxPeekAngle = 20f;
        private float peekSpeed = 5f;
        private float currentZRotation;
        // Peeking end
        
        // Crouching start
        private float crouchHeight = 1f;
        private float crouchTransitionSpeed = 10f;
        private float crouchSpeedMultiplier = 0.3f;
        
        Vector3 initialCameraPosition;
        private float currentHeight;
        float standingHeight;
        private bool IsCrouching => standingHeight - currentHeight > 0.1f;
        // Crouching end
        public event Action OnBeforeMove;
        
        internal float MovementSpeedMultiplier;

        private State state = State.Walking;
        private enum State
        {
            Walking,
            Sprinting,
            Sneaking,
            Peeking,
            Crouching,
        }

        public float Height
        {
            get => controller.height;
            set => controller.height = value;
        }

        CharacterController controller;
        Vector2 look;
        internal Vector3 velocity;
        
        PlayerInput playerInput;
        InputAction moveAction;
        InputAction lookAction;
        InputAction sprintAction;
        InputAction sneakAction;
        // InputAction peekAction;
        InputAction crouchAction;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();
            moveAction = playerInput.actions["move"];
            lookAction = playerInput.actions["look"];
            sprintAction = playerInput.actions["sprint"];
            sneakAction = playerInput.actions["sneak"];
            // peekAction = playerInput.actions["peek"];
            crouchAction = playerInput.actions["crouch"];
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            // Crouching
            initialCameraPosition = cameraTransform.localPosition;
            standingHeight = currentHeight = Height;
        }
        private void Update()
        {
            MovementSpeedMultiplier = 1f;
            
            UpdateStateFromInput();
            UpdateGravity();
            UpdateByState();
            UpdateDefault();
            UpdateLook();
        }

        private bool CanChangeState(State newState)
        {
            if (state == newState) return false;
            
            if (state == State.Sneaking && newState == State.Sprinting) return false;
            
            if (state == State.Crouching && (newState == State.Sprinting || newState == State.Sneaking)) return false;
            
            // if (state == State.Peeking && (newState == State.Crouching || newState == State.Sneaking || newState == State.Sprinting)) return false;
            
            return true;
        }

        private void UpdateStateFromInput()
        {
            State desiredState;
            
            if (sprintAction.ReadValue<float>() > 0)
            {
                desiredState = State.Sprinting;
            } else if (sneakAction.ReadValue<float>() > 0)
            {
                desiredState = State.Sneaking;
            }
            else if (crouchAction.ReadValue<float>() > 0)
            {
                desiredState = State.Crouching;
            }
            else
            {
                desiredState = State.Walking;
            }

            if (CanChangeState(desiredState))
            {
                state = desiredState;
            }
        }

        private void UpdateByState()
        {
            switch (state)
            {
                case State.Sprinting:
                    UpdateSprinting();
                    break;
                case State.Sneaking:
                    UpdateSneaking();
                    break;
                case State.Crouching:
                    UpdateCrouching();
                    // UpdatePeeking();
                    break;
                // case State.Peeking:
                //     break;
                default:
                    UpdateWalking();
                    break;
            }
        }

        private void UpdateSneaking()
        {
            MovementSpeedMultiplier *= 0.5f;
            
            ApplyBasicMovement();
        }

        private void UpdateWalking()
        {
            ApplyBasicMovement();
        }

        private void UpdateSprinting()
        {
            var forwardMovementFactor = Mathf.Clamp01(
                Vector3.Dot(transform.forward, velocity.normalized)   
            );
            var multiplier = Mathf.Lerp(1f, 2f, forwardMovementFactor);
            
            MovementSpeedMultiplier *= multiplier;

            ApplyBasicMovement();
        }

        private void UpdateGravity()
        {
            var gravity = Time.deltaTime * mass * Physics.gravity;
            velocity.y = controller.isGrounded ? -1f : velocity.y + gravity.y;
        }

        Vector3 GetMovementInput()
        {
            var moveInput = moveAction.ReadValue<Vector2>();
            var input = new Vector3();
            input += transform.forward * moveInput.y;
            input += transform.right * moveInput.x;
            input = Vector3.ClampMagnitude(input, 1);
            
            input *= movementSpeed * MovementSpeedMultiplier;
            
            return input;
        }

        private void ApplyBasicMovement()
        {
            var input = GetMovementInput();
            
            var factor = acceleration * Time.deltaTime;
            velocity.x = Mathf.Lerp(velocity.x, input.x, factor);
            velocity.z = Mathf.Lerp(velocity.z, input.z, factor);
            
            controller.Move(velocity * Time.deltaTime);
        }

        private void UpdateLook()
        {
            var lookInput = lookAction.ReadValue<Vector2>();
            look.x += lookInput.x * mouseSensitivity;
            look.y += lookInput.y * mouseSensitivity;

            look.y = Mathf.Clamp(look.y, -89f, 89f);
            
            cameraTransform.localRotation = Quaternion.Euler(-look.y, 0, 0);
            transform.localRotation = Quaternion.Euler(0, look.x, 0);
        }
        
        private void UpdateDefault()
        {
            // Determine target height based on current state
            float targetHeight = (state == State.Crouching) ? crouchHeight : standingHeight;

            // If standing up (not crouching), check for ceiling obstruction to prevent clipping
            if (IsCrouching && state != State.Crouching)
            {
                var castOrigin = transform.position + new Vector3(0, currentHeight / 2, 0);
                if (Physics.Raycast(castOrigin, Vector3.up, out RaycastHit hit, 0.2f))
                {
                    var distanceToCeiling = hit.point.y - castOrigin.y;
                    targetHeight = Mathf.Max(currentHeight + distanceToCeiling - 0.1f, crouchHeight);
                }
            }

            // Smoothly interpolate current height towards target height
            if (!Mathf.Approximately(targetHeight, currentHeight))
            {
                var crouchDelta = Time.deltaTime * crouchTransitionSpeed;
                currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchDelta);

                // Adjust camera local position based on height difference
                var halfHeightDifference = new Vector3(0, (standingHeight - currentHeight) / 2, 0);
                cameraTransform.localPosition = initialCameraPosition - halfHeightDifference;

                // Set CharacterController height
                Height = currentHeight;
            }

            // UpdatePeeking();
        }

        private void UpdateCrouching()
        {
            MovementSpeedMultiplier *= crouchSpeedMultiplier;
            
            ApplyBasicMovement();
        }

        // private void UpdatePeeking()
        // {
        //     var peekInput = peekAction.ReadValue<float>();
        //     var angle = (state == State.Peeking) ? peekInput * maxPeekAngle : 0;
        //                 
        //     currentZRotation = Mathf.Lerp(currentZRotation, angle, Time.deltaTime * peekSpeed);
        //     cameraPivot.localRotation = Quaternion.Euler(0f, 0f, currentZRotation);
        // }
    }
}