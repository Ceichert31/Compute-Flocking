using UnityEngine;

public class CursorLock : MonoBehaviour
{
    [SerializeField]
    private CursorLockMode defaultMode;

    private void Awake()
    {
        Cursor.lockState = defaultMode;
    }

    public void UnlockCursor(BoolEvent ctx)
    {
        if (ctx.Value)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
