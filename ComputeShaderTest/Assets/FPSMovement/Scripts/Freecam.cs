using System;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(InputController))]
public class Freecam : MonoBehaviour
{
    [Header("Free Cam Settings")]
    [SerializeField]
    private float flySpeed = 10f;
    
    
    private PlayerControls playerActions;
    private PlayerControls.FreecamActions freeCameraMovement;
    private Rigidbody rb;

    private bool isFreeCameraEnabled;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(ref PlayerControls action)
    {
        playerActions = action;
    }

    private void FixedUpdate()
    {
        if (!isFreeCameraEnabled) return;
        
        //rb.AddForce(playerActions.Move?.ReadValue<Vector2>() ?? Vector2.zero * flySpeed);
    }

    public void EnableFreecam(bool isEnabled)
    {
        isFreeCameraEnabled = isEnabled;
        
        if (isEnabled)
        {
            rb.useGravity = false;
    
        }
    }
}
