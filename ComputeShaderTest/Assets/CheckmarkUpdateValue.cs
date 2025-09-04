using System;
using UnityEngine;
using UnityEngine.UI;
public class CheckmarkUpdateValue : MonoBehaviour
{
    private Toggle toggle;

    [SerializeField]
    private UIEventChannel updateUIValuesEvent;
   
    [SerializeField]
    private UIEvent uiEvent;

    
    private void Start()
    {
        toggle = GetComponent<Toggle>();
        
        toggle.onValueChanged.AddListener(x =>
        {
            uiEvent.Value = toggle.isOn ? 1 : 0;
            updateUIValuesEvent.CallEvent(uiEvent);
        });
    }
    
    
}
