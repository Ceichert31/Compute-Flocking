using UnityEngine;

public class MazeRoom : MonoBehaviour
{
    public GameObject top;
    public GameObject right;
    public GameObject bottom;
    public GameObject left;
    [SerializeField]
    private GameObject floor;

    public void SetCurrent(bool isCurrent)
    {
        if (isCurrent)
        {
            floor.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            floor.GetComponent<MeshRenderer>().material.color = Color.black;
        }
    }
}
