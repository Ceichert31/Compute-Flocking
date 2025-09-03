using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    //Open and close UI event

    //Dotween menu open

    //Unlock cursor on other script

    //Pause simulation?

    [SerializeField]
    private GameObject devMenu;

    [SerializeField]
    private float menuOpenTime = 0.3f;

    public void OpenMenu(BoolEvent ctx)
    {
        if (ctx.Value)
        {
            //Tween menu open
            devMenu.transform.DOScaleX(1, menuOpenTime).SetEase(Ease.OutBack);
        }
        else
        {
            //Tween menu closed
            devMenu.transform.DOScaleX(0, menuOpenTime).SetEase(Ease.InOutCubic);
        }
    }
}
