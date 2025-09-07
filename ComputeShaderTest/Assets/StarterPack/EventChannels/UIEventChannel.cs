using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/UI Event Channel")]
public class UIEventChannel : GenericEventChannel<UIEvent> { }

[System.Serializable]
public struct UIEvent
{
    public UIEvent(UIElements element, float value)
    {
        UIElement = element;
        Value = value;
    }

    public UIElements UIElement;
    public float Value;
}

public enum UIElements
{
    SeparationSlider,
    AlignmentSlider,
    CohesionSlider,
    TerrainSlider,
    DebugCheckbox,
    BoidSpeedSlider,
    BoidDistanceSlider,
    BoidCountSlider,
    BoundrySlider,
    ResetButton,
}
