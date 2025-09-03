using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Game Object Event Channel")]
public class GameObjectEventChannel : GenericEventChannel<GameObjectEvent> {}

[System.Serializable]
public struct GameObjectEvent
{
    public GameObject Object;

    public GameObjectEvent(GameObject obj) => Object = obj;
}
