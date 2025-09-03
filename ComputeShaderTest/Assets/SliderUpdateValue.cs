using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderUpdateValue : MonoBehaviour
{
    private Slider slider;

    [SerializeField]
    private UIEventChannel uiOnValueChangeEvent;

    [SerializeField]
    private UIEvent uiEvent;

    [SerializeField]
    private TextMeshProUGUI sliderText;

    private void Start()
    {
        slider = GetComponent<Slider>();

        sliderText.text = slider.value.ToString("0.00");

        slider.onValueChanged.AddListener(
            (x) =>
            {
                sliderText.text = x.ToString("0.00");
                uiEvent.Value = x;
                uiOnValueChangeEvent.CallEvent(uiEvent);
            }
        );
    }
}
