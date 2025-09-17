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
    private RectTransform devMenu;

    [SerializeField]
    private float menuOpenTime = 0.3f;

    public void OpenMenu(BoolEvent ctx)
    {
        if (ctx.Value)
        {
            //Tween menu open
            DOTween.CompleteAll();
            devMenu.DOAnchorPosY(400, menuOpenTime).SetEase(Ease.OutBack);
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            DOTween.CompleteAll();
            //Tween menu closed
            devMenu.DOAnchorPosY(-400, menuOpenTime).SetEase(Ease.InOutCubic);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
