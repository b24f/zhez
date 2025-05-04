using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class Player: MonoBehaviour
    {
        public Transform cameraTransform;
        
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private float movementSpeed = 4f;
        [SerializeField] private float mass = 1f;
        [SerializeField] private float acceleration = 20f;

        public event Action OnBeforeMove;
        
        internal float movementSpeedMultiplier;

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

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();
            moveAction = playerInput.actions["move"];
            lookAction = playerInput.actions["look"];
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        private void Update()
        {
            UpdateGravity();
            UpdateMovement();
            UpdateLook();
        }

        private void UpdateGravity()
        {
            var gravity = Physics.gravity * mass * Time.deltaTime;
            velocity.y = controller.isGrounded ? -1f : velocity.y + gravity.y;
        }

        Vector3 GetMovementInput()
        {
            movementSpeedMultiplier = 1f;
            OnBeforeMove?.Invoke();
            
            var moveInput = moveAction.ReadValue<Vector2>();

            var input = new Vector3();
            input += transform.forward * moveInput.y;
            input += transform.right * moveInput.x;
            input = Vector3.ClampMagnitude(input, 1);
            
            input *= movementSpeed * movementSpeedMultiplier;
            
            return input;
        }

        private void UpdateMovement()
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
    }
}