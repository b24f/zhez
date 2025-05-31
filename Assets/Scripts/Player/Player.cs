using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;

namespace Player
{
    public class Player: MonoBehaviour
    {
        public Transform cameraTransform;
        
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private float movementSpeed = 4f;
        [SerializeField] private float mass = 1f;
        [SerializeField] private float acceleration = 20f;
        
        // Climbing start
        [SerializeField] private float climbingSpeed = 2f;
        // Climbing end
        
        // Sprinting start
        private const float MaxStamina = 100f;
        private const float StaminaDrainRate = 25f;
        private const float StaminaRegenRate = 15f;
        [SerializeField] private float staminaCooldownDuration = 5f;
        private bool staminaExhausted;
        private float staminaCooldownTimer;
        private float stamina;
        public float StaminaPercentage => Mathf.RoundToInt((stamina / MaxStamina) * 100f);
        // Sprinting end
        
        // Peeking start
        // [SerializeField] private Transform cameraPivot;
        // private float maxPeekAngle = 20f;
        // private float peekSpeed = 5f;
        // private float currentZRotation;
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

        public State? ForcedState = null;
        
        private State state = State.Walking;

        private State CurrentState
        {
            get => state;
            set
            {
                state = value;
                // velocity = Vector3.zero;
            }
        }
        public enum State
        {
            Walking,
            Sprinting,
            Sneaking,
            // Peeking,
            Crouching,
            Climbing,
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

        public static Player Current;

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
            
            Current = this;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            // Crouching
            initialCameraPosition = cameraTransform.localPosition;
            standingHeight = currentHeight = Height;
            
            stamina = MaxStamina; // Initialize stamina
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            int layer = hit.gameObject.layer;
            string layerName = LayerMask.LayerToName(layer);
            
            // Debug.Log($"Current State: {CurrentState}");
            ForcedState = layerName == "LongStairs" ? State.Sneaking : null;
        }
        
        private void Update()
        {
            if (GameState.Current.state == Types.State.Menu) return;
            
            MovementSpeedMultiplier = 1f;
            // Debug.Log($"Stamina: {stamina:F1}, Exhausted: {staminaExhausted}, Cooldown: {staminaCooldownTimer:F2}");

            UpdateStateFromInput();
            UpdateByState();
            UpdateLook();
            UpdateStaminaCooldown();
            UpdateGravity();
            UpdateDefault();
        }

        private bool CanChangeState(State newState)
        {
            if (CurrentState == newState) return false;
            
            if (CurrentState == State.Sneaking && newState == State.Sprinting) return false;
            
            if (CurrentState == State.Crouching && (newState == State.Sprinting || newState == State.Sneaking)) return false;
            
            // if (CurrentState == State.Peeking && (newState == State.Crouching || newState == State.Sneaking || newState == State.Sprinting)) return false;
            
            return true;
        }

        private void UpdateStateFromInput()
        {
            State desiredState;
            // Debug.Log($"ForcedState: {ForcedState}");
            if (ForcedState != null)
            {
                CurrentState = (State)ForcedState;
                return;
            }

            if (sprintAction.ReadValue<float>() > 0 && stamina > 0f && !staminaExhausted)
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
                CurrentState = desiredState;
            }
        }

        private void UpdateByState()
        {
            // Debug.Log($"Current State: {CurrentState}");
            switch (CurrentState)
            {
                case State.Sprinting:
                    UpdateSprinting();
                    break;
                case State.Sneaking:
                    UpdateSneaking();
                    break;
                case State.Crouching:
                    UpdateCrouching();
                    break;
                case State.Climbing:
                    UpdateClimbing();
                    break;
                default:
                    UpdateWalking();
                    break;
            }
        }

        private void UpdateClimbing()
        {
            var input = GetMovementInput(climbingSpeed, false);
            var forwardInputFactor = Vector3.Dot(transform.forward, input.normalized);
            if (forwardInputFactor > 0)
            {
                input.x *= .5f;
                input.z *= .5f;
                
                if (Mathf.Abs(input.y) > .2f)
                {
                    input.y = Mathf.Sign(input.y) * climbingSpeed;
                }
            }
            else
            {
                input.y = 0;
                input.x *= 3f;
                input.z *= 3f;
            }
            
            var factor = acceleration * Time.deltaTime;
            velocity = Vector3.Lerp(velocity, input, factor);
            
            controller.Move(velocity * Time.deltaTime);
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
            if (staminaExhausted)
            {
                CurrentState = State.Walking;
                return;
            }

            var forwardMovementFactor = Mathf.Clamp01(
                Vector3.Dot(transform.forward, velocity.normalized)   
            );
            var multiplier = Mathf.Lerp(1f, 2f, forwardMovementFactor);
            
            MovementSpeedMultiplier *= multiplier;
            
            stamina -= StaminaDrainRate * Time.deltaTime;
            stamina = Mathf.Max(0f, stamina);

            if (stamina <= 0f)
            {
                staminaExhausted = true;
                staminaCooldownTimer = staminaCooldownDuration;
                CurrentState = State.Walking;

                return;
            }

            MakeNoise();

            ApplyBasicMovement();
        }

        private void UpdateGravity()
        {
            var gravity = Time.deltaTime * mass * Physics.gravity;
            velocity.y = controller.isGrounded ? -1f : velocity.y + gravity.y;
        }

        Vector3 GetMovementInput(float speed, bool horizontal = true)
        {
            var moveInput = moveAction.ReadValue<Vector2>();
            var input = new Vector3();
            
            var referenceTransform = horizontal ? transform : cameraTransform; 
            
            input += referenceTransform.forward * moveInput.y;
            input += referenceTransform.right * moveInput.x;
            input = Vector3.ClampMagnitude(input, 1);
            
            input *= speed * MovementSpeedMultiplier;
            
            return input;
        }

        private void ApplyBasicMovement()
        {
            var input = GetMovementInput(movementSpeed);
            
            var factor = acceleration * Time.deltaTime;
            velocity.x = Mathf.Lerp(velocity.x, input.x, factor);
            velocity.z = Mathf.Lerp(velocity.z, input.z, factor);
            // Debug.Log($"Velocity: {velocity}");
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
            float targetHeight = (CurrentState == State.Crouching) ? crouchHeight : standingHeight;

            // If standing up (not crouching), check for ceiling obstruction to prevent clipping
            if (IsCrouching && CurrentState != State.Crouching)
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
        
        private void RegenerateStamina()
        {
            stamina += StaminaRegenRate * Time.deltaTime;
            stamina = Mathf.Min(MaxStamina, stamina);
        }
        
        private void UpdateStaminaCooldown()
        {
            if (staminaExhausted)
            {
                staminaCooldownTimer -= Time.deltaTime;
                if (staminaCooldownTimer <= 0f)
                {
                    staminaExhausted = false;
                }
            }
            else if (CurrentState != State.Sprinting)
            {
                RegenerateStamina();
            }
        }

        // private void UpdatePeeking()
        // {
        //     var peekInput = peekAction.ReadValue<float>();
        //     var angle = (CurrentState == State.Peeking) ? peekInput * maxPeekAngle : 0;
        //                 
        //     currentZRotation = Mathf.Lerp(currentZRotation, angle, Time.deltaTime * peekSpeed);
        //     cameraPivot.localRotation = Quaternion.Euler(0f, 0f, currentZRotation);
        // }
        
        // [SerializeField] private float noiseRadius = 10f;
        // private readonly Collider[] colliders = new Collider[10];

        private void MakeNoise()
        {
            // var count = Physics.OverlapSphereNonAlloc(transform.position, noiseRadius, colliders);
            //
            // for (var i = 0; i < count; i++)
            // {
            //     if (colliders[i].CompareTag("Player"))
            //     {
            //     }
            // }
            
            Zhez.Zhez.Current.OnPlayerMadeNoise(transform.position);
        }
    }
}