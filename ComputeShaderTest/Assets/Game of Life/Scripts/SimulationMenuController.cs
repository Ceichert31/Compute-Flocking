using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimulationMenuController : MonoBehaviour
{
    [SerializeField] private BoolEventChannel openMenuEvent;
    
    private PlayerControls playerControls;
    private bool openMenu = false;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnTab(InputAction.CallbackContext ctx)
    {
        openMenu = !openMenu;
        
        openMenuEvent.CallEvent(new(openMenu));
    }

    private void OnEnable()
    {
        playerControls.Enable();
        
        playerControls.UI.DevMenu.performed += OnTab;
    }
    private void OnDisable()
    {
        playerControls.Disable();
        
        playerControls.UI.DevMenu.performed += OnTab;
    }
}
