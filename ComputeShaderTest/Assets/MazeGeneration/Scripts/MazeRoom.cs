using UnityEngine;

public class MazeRoom : MonoBehaviour
{
    public GameObject top;
    public GameObject right;
    public GameObject bottom;
    public GameObject left;

    public void SetCurrent(bool isCurrent)
    {
        if (isCurrent)
        {
            top.GetComponent<Material>().SetColor("_BaseColor", Color.red);
            right.GetComponent<Material>().SetColor("_BaseColor", Color.red);
            bottom.GetComponent<Material>().SetColor("_BaseColor", Color.red);
            left.GetComponent<Material>().SetColor("_BaseColor", Color.red);
        }
        else
        {
            top.GetComponent<Material>().SetColor("_BaseColor", Color.black);
            right.GetComponent<Material>().SetColor("_BaseColor", Color.black);
            bottom.GetComponent<Material>().SetColor("_BaseColor", Color.black);
            left.GetComponent<Material>().SetColor("_BaseColor", Color.black);
        }
    }
}
