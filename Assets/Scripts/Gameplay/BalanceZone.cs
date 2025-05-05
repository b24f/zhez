using UnityEngine;
using Player;

public class BalanceZone : MonoBehaviour
{
    [SerializeField] private Transform PlayerCamera;
    [SerializeField] private GameObject FallenTree;
    [SerializeField] private Rigidbody PlayerRB;
    
    private PlayerMovement playerMovement;
    private ScreenFader fader;

    // Player tilt
    [SerializeField] private float tiltSpeed = 30f;
    [SerializeField] private float playerTiltSpeed = 50f;
    private const float MaxTilt = 30f;
    private float currentTilt = 0f;
    private int autoTiltDirection = -1;
    private float playerInput = 0f;
    private bool playerInZone = false;

    // Tree fall
    [SerializeField] private float treeRotationDuration = 2f;
    private const float MaxAngle = -30f;
    private float elapsed = 0f;
    private bool finished = false;

    private void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        fader = FindObjectOfType<ScreenFader>();
    }

    // private void FixedUpdate()
    // {
    //     if (!playerInZone) return;
    //
    //     HandleBalance();
    // }

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
        if (Mathf.Abs(currentTilt) > MaxTilt)
        {
            Fall();
        }

        // Apply to camera
        PlayerCamera.localRotation = Quaternion.Euler(
            PlayerCamera.localRotation.eulerAngles.x,
            PlayerCamera.localRotation.eulerAngles.y,
            currentTilt
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Entered Balance Zone");
            Fall();
        }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         Debug.Log("Left Balance Zone");
    //         PlayerCamera.localRotation = Quaternion.Euler(
    //             PlayerCamera.localRotation.eulerAngles.x,
    //             PlayerCamera.localRotation.eulerAngles.y,
    //             30f
    //         );
    //         
    //         Fall();
    //     }
    // }
    
    private void Fall()
    {
        // if (finished) return;
        
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / treeRotationDuration);

        float angle = Mathf.Sin(t * Mathf.PI * 0.5f) * MaxAngle;
        
        FallenTree.transform.localRotation = Quaternion.Euler(
            FallenTree.transform.localEulerAngles.x,
            FallenTree.transform.localEulerAngles.y,
            angle);
        
        PlayerRB.isKinematic = false;
        
        fader.FadeOut();

        if (t >= 1f)
        {
            finished = true;
        }
    }

    private void SetVerticalMovement(bool inZone)
    {
        if (playerMovement != null)
        {
            playerMovement.verticalMovement = inZone;
            playerInZone = inZone;
        }
    }
}
