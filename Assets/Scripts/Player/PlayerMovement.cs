using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform playerCamera;

    [SerializeField] private float speed = 3f;
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float sensitivity = 400f;

    private float xRotation;
    private Vector3 velocity;

    [HideInInspector]
    public bool verticalMovement = false;
    // public float gravity = -30f;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector3 move = Vector3.zero;

        // Player Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        // Camera Movement
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        if (verticalMovement)
        {
            move = transform.forward * z;
        }
        else
        {
            move = transform.right * x + transform.forward * z;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            transform.Rotate(Vector3.up * mouseX);
        }
        
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        move = (move * speed) + velocity;
        
        controller.Move(move * Time.deltaTime);
    }
}
