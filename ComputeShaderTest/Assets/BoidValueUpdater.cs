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
            case UIElements.TerrainSlider:
                //update with value
                instancedFlocking.GroundAvoidanceWeight = ctx.Value;
                break;
            case UIElements.DebugCheckbox:
                if (Mathf.Approximately(ctx.Value, 1))
                {
                    instancedFlocking.isDebugEnabled = true;
                    instancedFlocking.isTerrainDebugEnabled = true;
                }
                else
                {
                    instancedFlocking.isDebugEnabled = false;
                    instancedFlocking.isTerrainDebugEnabled = false;    
                }
                break;
            case UIElements.BoidSpeedSlider:
                instancedFlocking.BoidSpeed = ctx.Value;
                break;
            case UIElements.BoidDistanceSlider:
                instancedFlocking.NeighbourDistance = ctx.Value;
                break;
            case UIElements.BoidCountSlider:
                instancedFlocking.BoidsCount = (int)ctx.Value;
                instancedFlocking.ResetBoids();
                break;
        }
    }
}
