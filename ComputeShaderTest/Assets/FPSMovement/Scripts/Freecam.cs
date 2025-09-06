using System;
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
    private float sensitivity = 5f;
    
    private float currentSpeed;

    private bool isFreeCameraEnabled;

    private Camera cam;
    
    private PlayerControls playerControls;

    private float lookRotation;
    

    private void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }
    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void Initialize(PlayerControls playerControls)
    {
        //this.playerControls = playerControls;
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

    private void Move()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : flySpeed;
        transform.Translate(input * (currentSpeed * Time.deltaTime));
    }
    private void Look()
    {
        //Read mouse input
        Vector2 lookForce = playerControls.Movement.Look?.ReadValue<Vector2>() ?? Vector2.zero;

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
