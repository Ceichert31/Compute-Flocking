using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Void Event Channel")]
public class VoidEventChannel : GenericEventChannel<UIEvent> { }

[System.Serializable]
public struct VoidEvent { }
