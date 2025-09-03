using UnityEngine;

[RequireComponent(typeof(InstancedFlocking))]
public class BoidValueUpdater : MonoBehaviour
{
    private InstancedFlocking instancedFlocking;

    private void Start()
    {
        instancedFlocking = GetComponent<InstancedFlocking>();
    }

    /// <summary>
    /// Updates a value on the boid manager script
    /// </summary>
    /// <param name="ctx">The value and UI element type</param>
    public void UpdateValue(UIEvent ctx)
    {
        switch (ctx.UIElement)
        {
            case UIElements.SeparationSlider:
                //update with value
                instancedFlocking.SeperationWeight = ctx.Value;
                break;
            case UIElements.CohesionSlider:
                //update with value
                instancedFlocking.CohesionWeight = ctx.Value;
                break;
            case UIElements.AlignmentSlider:
                //update with value
                instancedFlocking.AlignmentWeight = ctx.Value;
                break;
        }
    }
}
