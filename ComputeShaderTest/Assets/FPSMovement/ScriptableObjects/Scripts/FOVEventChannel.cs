using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Event Channel/Field of View", fileName = "FOVEventChannel")]
public class FOVEventChannel : ScriptableObject
{
    public delegate void FOVController(float targetFOV, float timeBeforeDecay);
    public event FOVController FOVControllerUpdate;
    public void IncreaseFOV(float targetFOV, float timeBeforeDecay) => FOVControllerUpdate?.Invoke(targetFOV, timeBeforeDecay);
}
