using TMPro;
using UnityEngine;

public class FPSTextDisplay : MonoBehaviour
{
    private TextMeshProUGUI fpsText;

    [SerializeField]
    private float refreshRate = 0.5f;
    private float refreshTimer;

    private float fps;

    private void Start()
    {
        fpsText = GetComponent<TextMeshProUGUI>();

        refreshTimer = Time.unscaledDeltaTime + refreshRate;
    }

    private void Update()
    {
        if (Time.time < refreshTimer)
            return;

        fps = (int)(1f / Time.unscaledDeltaTime);

        fpsText.text = $"FPS: {fps}";

        refreshTimer = Time.time + refreshRate;
    }
}
