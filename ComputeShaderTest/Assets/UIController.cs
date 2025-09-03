using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private UIEventChannel uiElementUpdatedEvent;

    private UIEvent uiEvent;

    [SerializeField]
    private Slider separationSlider;

    UnityAction<float> separationAction;

    private void Awake()
    {
        separationSlider.onValueChanged.AddListener(separationAction);

        separationAction += UpdateSliderInfo;
    }

    private void UpdateSliderInfo(float value)
    {
        //Send event to event bus with value and UI element
        //Update boid manager's separation value
        //Update UI
    }
}
