using UnityEngine;

public class CursorLock : MonoBehaviour
{
    [SerializeField] private CursorLockMode defaultMode;

    private void Awake()
    {
        Cursor.lockState = defaultMode;
    }
}
