using UnityEngine;

public class BalanceZone : MonoBehaviour
{
    public Transform playerCamera;
    private PlayerMovement playerMovement;

    public MeshCollider treeCollider;
    
    private ScreenFader fader;

    public float tiltSpeed = 30f;
    public float playerTiltSpeed = 50f;
    public float maxTilt = 30f;

    private float currentTilt = 0f;
    private int autoTiltDirection = -1;
    private float playerInput = 0f;
    private bool playerInZone = false;

    private void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>(); // Cache the reference at start
        fader = FindObjectOfType<ScreenFader>();
    }

    private void Update()
    {
        if (!playerInZone) return;

        HandleBalance();
    }

    private void HandleBalance()
    {
        // Get player input
        if (Input.GetKey(KeyCode.A))
        {
            playerInput = 1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            playerInput = -1f;
        }
        else
        {
            playerInput = 0f;
        }

        // Auto tilt
        currentTilt += autoTiltDirection * tiltSpeed * Time.deltaTime;

        // Player tilt
        currentTilt += playerInput * playerTiltSpeed * Time.deltaTime;

        // Flip direction at 0
        if ((autoTiltDirection == -1 && currentTilt >= 0f) ||
            (autoTiltDirection == 1 && currentTilt <= 0f))
        {
            autoTiltDirection *= -1;
        }

        // Clamp
        currentTilt = Mathf.Clamp(currentTilt, -60f, 60f);
        
        // Fall
        if (Mathf.Abs(currentTilt) > maxTilt)
        {
            playerMovement.gravity = -9.81f * 3f;
            Destroy(treeCollider);
            fader.FadeOut();
        }

        // Apply to camera
        playerCamera.localRotation = Quaternion.Euler(
            playerCamera.localRotation.eulerAngles.x,
            playerCamera.localRotation.eulerAngles.y,
            currentTilt
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetVerticalMovement(true);
        }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         SetVerticalMovement(false);
    //     }
    // }

    // private void ResetTilt()
    // {
    //     currentTilt = 0f;
    //     playerCamera.localRotation = Quaternion.Euler(
    //         playerCamera.localRotation.eulerAngles.x,
    //         playerCamera.localRotation.eulerAngles.y,
    //         0f
    //     );
    // }

    private void SetVerticalMovement(bool inZone)
    {
        if (playerMovement != null)
        {
            playerMovement.verticalMovement = inZone;
            playerInZone = inZone;
        }
    }
}
