using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(InputController))]
public class Freecam : MonoBehaviour
{
    [Header("Free Cam Settings")]
    [SerializeField]
    private float flySpeed = 10f;
    [SerializeField]
    private float sprintSpeed = 20f;
    [SerializeField]
    private float verticalSpeed = 5f;

    [SerializeField]
    private float sensitivity = 5f;

    private float currentSpeed;

    private bool isFreeCameraEnabled;

    private Camera cam;
    
    private PlayerControls playerControls;
    private PlayerControls.FreecamActions freecamActions;
    
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float lookRotation;
    private Vector3 verticalMovement;

    private void Awake()
    {
        cam = GetComponentInChildren<Camera>();
       
        playerControls = new PlayerControls();
        freecamActions = playerControls.Freecam;
        currentSpeed = flySpeed;
    }

    public void PauseMovement(bool pause)
    {
        if (pause)
        {
            freecamActions.Disable();
        }
        else
        {
            freecamActions.Enable();
        }
    }
    
    private void OnEnable()
    {
        freecamActions.Enable();
        freecamActions.Move.performed += OnMove;
        freecamActions.Move.canceled += OnMove;
        freecamActions.Dash.performed += OnDash;
        freecamActions.Dash.canceled += OnDash;
        freecamActions.Up.performed += MoveUp;
        freecamActions.Up.canceled += MoveUp;
        freecamActions.Down.performed += MoveDown;
        freecamActions.Down.canceled += MoveDown;
    }
    private void OnDisable()
    {
        freecamActions.Disable();
        freecamActions.Move.performed -= OnMove;
        freecamActions.Move.canceled -= OnMove;
        freecamActions.Dash.performed -= OnDash;
        freecamActions.Dash.canceled -= OnDash;
        freecamActions.Up.performed -= MoveUp;
        freecamActions.Up.canceled -= MoveUp;
        freecamActions.Down.performed -= MoveDown;
        freecamActions.Down.canceled -= MoveDown;
    }
    private void Update()
    {
        if (!isFreeCameraEnabled) return;
        Move();
    }

    private void LateUpdate()
    {
        if (!isFreeCameraEnabled) return;
        Look();
    }

    public void EnableFreecam(bool isEnabled)
    {
        isFreeCameraEnabled = isEnabled;
    }
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.performed ? context.ReadValue<Vector2>() : Vector2.zero;
    }
    private void MoveUp(InputAction.CallbackContext context)
    {
        verticalMovement = context.performed ? Vector3.up : Vector3.zero;
    }
    private void MoveDown(InputAction.CallbackContext context)
    {
        verticalMovement = context.performed ? Vector3.down : Vector3.zero;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        currentSpeed = context.performed ? sprintSpeed : flySpeed;
    }

    private void Move()
    {
        Vector3 forwardMovement = Vector3.Normalize(transform.forward) * (moveInput.y * currentSpeed * Time.deltaTime);
        Vector3 rightMovement = Vector3.Normalize(transform.right) * (moveInput.x * currentSpeed * Time.deltaTime);
        
        transform.position += forwardMovement + rightMovement + (verticalMovement * (verticalSpeed * Time.deltaTime));
    }
    private void Look()
    {
        //Read mouse input
        Vector2 lookForce = freecamActions.Look?.ReadValue<Vector2>() ?? Vector2.zero;

        //Turn the player with the X-input
        gameObject.transform.Rotate(lookForce.x * sensitivity * Vector3.up / 100);

        //Add Y-input multiplied by sensitivity to float
        lookRotation += (-lookForce.y * sensitivity / 100);

        //Clamp the look rotation so the player can't flip the cameras
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);

        //Set cameras rotation
        cam.transform.eulerAngles = new Vector3(
            lookRotation,
            cam.transform.eulerAngles.y,
            cam.transform.eulerAngles.z
        );
    }
}
