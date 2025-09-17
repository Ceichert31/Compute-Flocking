using UnityEngine;

public class MazeRoom : MonoBehaviour
{
    public GameObject top;
    public GameObject right;
    public GameObject bottom;
    public GameObject left;
    [SerializeField]
    private MeshRenderer floor;

    public void SetCurrent(bool isCurrent)
    {
        floor.material.color = isCurrent ? Color.green : Color.black;
    }
    public void SetEnd()
    {
        floor.material.color = Color.red;
    }
}
