using UnityEngine;

public class CameraTilt : MonoBehaviour
{
    [Header("Camera Tilt ")]
    [Tooltip("The player Camera")]
    [SerializeField] private Transform mainCamera;

    [Header("Camera Tilt Settings")]
    [Tooltip("Controls how fast the camera returns to the origin position")]
    [SerializeField] private float tiltInSpeed = 10f;

    [Tooltip("Controls how fast the camera moves to the tilted position")]
    [SerializeField] private float tiltOutSpeed = 9f;

    [Tooltip("Controls how far the camera tilts when moving")]
    [SerializeField] private float tiltAmount = -4f;

    private InputController inputController;
    private Rigidbody rb;

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    private bool applyMovementEffects => inputController.ApplyMovementEffects;
    private bool isGrounded => inputController.IsGrounded;
    private Vector2 moveInput => inputController.MoveInput;

    private void Start()
    {
        inputController = GetComponent<InputController>();

        rb = GetComponent<Rigidbody>();

        if (mainCamera == null)
            mainCamera = transform.GetChild(0).GetChild(0);
    }

    private void Update()
    {
        //Guard Clause to prevent applying affect when not ideal
        if (applyMovementEffects || !isGrounded)
            return;

        TiltUpdate();
    }
    /// <summary>
    /// Applies camera tilt based on player movement
    /// </summary>
    void TiltUpdate()
    {
        bool doTilt = false;

        //If the player is moving above a certain velocity, set the current rotation to the tilt amount
        if (moveInput.x != 0 && rb.linearVelocity.magnitude > 2)
            doTilt = true;

        if (doTilt)
        {
            if (moveInput.x < 0)
                currentRotation = new(0, 0, -tiltAmount);
            else
                currentRotation = new(0, 0, tiltAmount);
        }
        //Return to origin position
        currentRotation = Vector3.Lerp(currentRotation, Vector3.zero, tiltOutSpeed * Time.deltaTime);

        //Rotate camera to tilted position
        targetRotation = Vector3.Lerp(targetRotation, currentRotation, tiltInSpeed * Time.deltaTime);
        mainCamera.transform.localRotation = Quaternion.Euler(targetRotation);
    }
}
