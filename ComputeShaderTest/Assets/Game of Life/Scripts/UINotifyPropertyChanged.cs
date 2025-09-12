using System;
using UnityEngine;
using UnityEngine.UI;

public class UINotifyPropertyChanged : MonoBehaviour
{
    private Slider slider;
    private Button button;
    private Toggle toggle;
    
    [Header("Event Channel References")]
    [SerializeField]
    private FloatEventChannel floatEventChannel;
    [SerializeField]
    private BoolEventChannel boolEventChannel;
    [SerializeField]
    private VoidEventChannel voidEventChannel;

    private FloatEvent floatEvent;
    private BoolEvent boolEvent;
    private VoidEvent voidEvent;

    private void Start()
    {
        if (TryGetComponent(out slider))
        {
            slider.onValueChanged.AddListener(x =>
            {
                floatEvent.FloatValue = slider.value;
                floatEventChannel.CallEvent(floatEvent);
            });
        }
        else if (TryGetComponent(out button))
        {
            button.onClick.AddListener(() =>
            {
                voidEventChannel.CallEvent(voidEvent);
            });
        }
        else if (TryGetComponent(out toggle))
        {
            toggle.onValueChanged.AddListener(x =>
            {
                boolEvent.Value = toggle.isOn;
                boolEventChannel.CallEvent(boolEvent);
            });
        }
        else
        {
            throw new Exception("UINotifyPropertyChanged needs to be attached to a UI Element");
        }
    }
}
