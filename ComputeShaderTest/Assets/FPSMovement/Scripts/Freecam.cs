using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Freecam : MonoBehaviour
{
    
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(PlayerControls.MovementActions action)
    {
        
    }

    public void EnableFreecam(bool isEnabled)
    {
        if (isEnabled)
        {
            rb.useGravity = false;
            
        }
    }
}
