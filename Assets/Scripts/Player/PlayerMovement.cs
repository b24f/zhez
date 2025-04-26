using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform playerCamera;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public float speed = 3f;

    float xRotation = 0f;
    public float sensitivity = 400f;

    Vector3 velocity;
    bool isGrounded;

    [HideInInspector]
    public bool verticalMovement = false;
    public float gravity = -9.81f;

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

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

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

        controller.Move(Time.deltaTime * speed * move);
        
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
