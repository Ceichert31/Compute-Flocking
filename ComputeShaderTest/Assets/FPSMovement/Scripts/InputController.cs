using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [Header("Event Channel Reference")]
    [SerializeField]
    private FOVEventChannel fovEventChannel;

    [SerializeField]
    private BoolEventChannel expandDevMenuEvent;
    private BoolEvent boolEvent;

    [Header("Camera Settings")]
    [SerializeField]
    private float sensitivity = 15f;

    [SerializeField]
    private float minSensitivity = 1f;

    [SerializeField]
    private float maxSensitivity = 30f;
    public float Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = Mathf.Clamp(value, minSensitivity, maxSensitivity); }
    }

    [Header("Movement Settings")]
    [Tooltip("The speed the player moves at")]
    [SerializeField]
    private float walkSpeed = 60f;

    [SerializeField]
    private bool advancedSettings;

    [Tooltip("The maximum angle the player can walk up without losing speed")]
    [SerializeField]
    private float maxSlopeAngle = 45f;

    [Tooltip("The height the floating rigidbody is offset from the ground")]
    [SerializeField]
    private float heightOffset = 1f;

    [Tooltip("How much is added to the heightOffset")]
    [SerializeField]
    private float offsetRayDistance = 1f;

    [Tooltip("Adds force to the total Y-offset")]
    [SerializeField]
    private float offsetStrength = 200f;

    [Tooltip("Affects how controlled the offset is")]
    [SerializeField]
    private float offsetDamper = 10f;

    [Tooltip("Adds more drag to the players velocity")]
    [SerializeField]
    private float dragRate = 5f;

    [Tooltip("The layers the player can walk on")]
    [SerializeField]
    private LayerMask groundLayer;

    [Header("Dash Settings")]
    [SerializeField]
    private float dashCooldown = 1f;

    [SerializeField]
    private float dashForce = 50f;

    [SerializeField]
    private float fovIncrease = 35f;

    [SerializeField]
    private float fovDecayTime = 2f;

    //private bool canDash = true;

    //Input References
    private PlayerControls playerControls;
    private PlayerControls.MovementActions playerMovement;
    private PlayerControls.UIActions playerUIActions;

    //Physics References
    private Rigidbody rb;

    private RaycastHit groundHit;

    //Camera References
    private Camera cam;

    private float lookRotation;

    private Vector2 moveInput;

    private bool isFreecamEnabled;

    //Freecam steps
    //Disable gravity
    //Use Q + E for up and down
    //Slow players velocity when key is released

    /// <summary>
    /// Position before freecam
    /// </summary>
    private Vector3 previousPosition;

    //Input controller
    //Try get freecam script on button press

    //Getters
    private bool isGrounded;
    private bool isMoving;
    private bool applyMovementEffects;
    private bool isUIOpen;
    public bool IsGrounded
    {
        get { return isGrounded; }
    }
    public bool IsMoving
    {
        get { return isMoving; }
    }
    public bool ApplyMovementEffects
    {
        get { return applyMovementEffects; }
    }
    public Vector2 MoveInput
    {
        get { return moveInput; }
    }

    private bool hasFreecamComponent;
    private Freecam freeCam;
    private void Awake()
    {
        //Initialize Controls
        playerControls = new PlayerControls();
        playerMovement = playerControls.Movement;
        playerUIActions = playerControls.UI;

        rb = GetComponent<Rigidbody>();

        cam = GetComponentInChildren<Camera>();

        if (TryGetComponent(out freeCam))
        {
            hasFreecamComponent = true;
        }

        //To confirm the ray distance is longer than the height offset
        offsetRayDistance += heightOffset;
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            -Vector3.up,
            out groundHit,
            offsetRayDistance,
            groundLayer
        );
    }

    //Movement
    private void FixedUpdate() => Move();

    private Vector3 MoveDirection()
    {
        //Read player input
        moveInput = playerMovement.Move?.ReadValue<Vector2>() ?? Vector2.zero;

        //Project two vectors onto an orthogonal plane and multiply them by the players x and y inputs
        Vector3 moveDirection = (
            Vector3.ProjectOnPlane(transform.forward, Vector3.up) * moveInput.y
            + Vector3.ProjectOnPlane(transform.right, Vector3.up) * moveInput.x
        );
        //Normalize the two projected inputs added together to get the movement direction
        moveDirection.Normalize();

        //Returns unit vector
        return moveDirection;
    }

    private void Move()
    {
        if (!isGrounded)
            return;
        if (isFreecamEnabled)
            return;

        //Check if moving
        isMoving = playerMovement.Move.inProgress;

        //Apply walk speed to the movement vector
        Vector3 moveForce = MoveDirection() * walkSpeed;

        //Find the angle between the players up position and the groundHit
        float slopeAngle = Vector3.Angle(Vector3.up, groundHit.normal);

        //Set to (0, 0, 0)
        Vector3 yOffsetForce = Vector3.zero;

        //If surface angle is within max angle
        if (slopeAngle <= maxSlopeAngle)
        {
            //Find difference between ground distance and the offset
            float yOffsetError = (heightOffset - groundHit.distance);

            //Find the dot product of vector3.up and of the players velocity
            float yOffsetVelocity = Vector3.Dot(Vector3.up, rb.linearVelocity);

            //Set the offset force of the floating rigidbody
            yOffsetForce =
                Vector3.up * (yOffsetError * offsetStrength - yOffsetVelocity * offsetDamper);
        }
        //Calculate the combined force between direction and offset
        Vector3 combinedForces = moveForce + yOffsetForce;

        //Calculate damping forces by multiplying the drag and player velocity
        Vector3 dampingForces = rb.linearVelocity * dragRate;

        //Add forces to rigidbody
        rb.AddForce((combinedForces - dampingForces) * (100 * Time.fixedDeltaTime));
    }

    //Camera movement
    private void LateUpdate() => Look();

    private void Look()
    {
        if (isFreecamEnabled) return;
        
        //Check if player is looking too far up or down
        applyMovementEffects = lookRotation > 80 || lookRotation < -80;

        //Read mouse input
        Vector2 lookForce = playerMovement.Look?.ReadValue<Vector2>() ?? Vector2.zero;

        //Turn the player with the X-input
        gameObject.transform.Rotate(lookForce.x * sensitivity * Vector3.up / 100);

        //Add Y-input multiplied by sensitivity to float
        lookRotation += (-lookForce.y * sensitivity / 100);

        //Clamp the look rotation so the player can't flip the camera
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);

        //Set cameras rotation
        cam.transform.eulerAngles = new Vector3(
            lookRotation,
            cam.transform.eulerAngles.y,
            cam.transform.eulerAngles.z
        );
    }

    /// <summary>
    /// Enables/Disables freecam and resets the players position
    /// </summary>
    /// <param name="ctx"></param>
    private void EnableFreecam(InputAction.CallbackContext ctx)
    {
        if (!hasFreecamComponent) return;
        
        isFreecamEnabled = !isFreecamEnabled;

        if (isFreecamEnabled)
        {
            previousPosition = transform.position;
            freeCam.EnableFreecam(isFreecamEnabled);
            rb.isKinematic = true;
            playerMovement.Disable();
        }
        else
        {
            transform.position = previousPosition;
            freeCam.EnableFreecam(isFreecamEnabled);
            rb.isKinematic = false;
            playerMovement.Enable();
        }
    }

    private void DevMenu(InputAction.CallbackContext ctx)
    {
        isUIOpen = !isUIOpen;

        if (isUIOpen)
        {
            playerMovement.Disable();
            
            if (isFreecamEnabled)
                freeCam.PauseMovement(true);
        }
        else
        {
            if (!isFreecamEnabled)
                playerMovement.Enable();
            else
                freeCam.PauseMovement(false);
        }

        boolEvent.Value = isUIOpen;

        expandDevMenuEvent.CallEvent(boolEvent);
    }

    private void OnEnable()
    {
        playerMovement.Enable();
        playerUIActions.Enable();
        playerControls.Freecam.Enable();

        playerControls.Freecam.Freecam.performed += EnableFreecam;
        playerUIActions.DevMenu.performed += DevMenu;
    }

    private void OnDisable()
    {
        playerMovement.Disable();
        playerUIActions.Disable();
        playerControls.Freecam.Disable();

        playerControls.Freecam.Freecam.performed -= EnableFreecam;
        playerUIActions.DevMenu.performed -= DevMenu;
    }
}
