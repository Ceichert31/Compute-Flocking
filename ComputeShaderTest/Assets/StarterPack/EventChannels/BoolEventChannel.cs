using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Bool Event Channel")]
public class BoolEventChannel : GenericEventChannel<BoolEvent> { }
[System.Serializable]
public struct BoolEvent
{
    public bool Value;
    public BoolEvent(bool instance) => Value = instance;
}
