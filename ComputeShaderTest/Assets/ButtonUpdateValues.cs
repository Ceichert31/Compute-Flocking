using UnityEngine;
using UnityEngine.UI;

public class ButtonUpdateValues : MonoBehaviour
{
    private Button button;

    [SerializeField]
    private UIEventChannel updateUIValuesEvent;
   
    [SerializeField]
    private UIEvent uiEvent;

    void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(() =>
        {
            updateUIValuesEvent.CallEvent(uiEvent);
        });
    }
}
