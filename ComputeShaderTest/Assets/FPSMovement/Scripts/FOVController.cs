using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class FOVController : MonoBehaviour
{
    [Header("Scriptable Object Reference")]
    [SerializeField] private FOVEventChannel fovEventChannel;

    [Header("Field of View Settings")]
    [SerializeField] private float fieldOfViewScale = 10f;
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float maxFOV = 100f;
    [SerializeField] private float smoothTime = 0.2f;

    private Camera mainCamera;

    private Rigidbody rb;

    private float fieldOfViewTarget;

    private float additionalFOV;

    private float currentVelocity;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        mainCamera = GetComponentInChildren<Camera>();
    }
    private void Update()
    {
        float scaledVelocity = rb.linearVelocity.magnitude / fieldOfViewScale;

        fieldOfViewTarget = baseFOV + scaledVelocity;

        Mathf.Clamp(fieldOfViewTarget, baseFOV, maxFOV);

        mainCamera.fieldOfView = Mathf.SmoothDamp(mainCamera.fieldOfView, fieldOfViewTarget + additionalFOV, ref currentVelocity, smoothTime);
    }
    /// <summary>
    /// Called by event channel
    /// </summary>
    /// <param name="targetFOV"></param>
    /// <param name="timeBeforeDecay"></param>
    private void IncreaseFOV(float targetFOV, float timeBeforeDecay)
    {
        additionalFOV = targetFOV;
        StartCoroutine(DecreaseFOV(targetFOV, timeBeforeDecay));
    }
    IEnumerator DecreaseFOV(float targetFOV, float timeBeforeDecay)
    {
        yield return new WaitForSeconds(timeBeforeDecay);
        additionalFOV -= targetFOV;
    }
    private void OnEnable() => fovEventChannel.FOVControllerUpdate += IncreaseFOV;
    private void OnDisable() => fovEventChannel.FOVControllerUpdate -= IncreaseFOV;
}
