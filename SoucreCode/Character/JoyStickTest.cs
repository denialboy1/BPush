using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoyStickTest : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    #region Private Fields

    private Image bgImg;
    private Image joystickImg;
    private Vector3 inputVector;

    #endregion

    #region Private Method

    void Start()
    {
        bgImg = GetComponent<Image>();
        joystickImg = transform.GetChild(0).GetComponent<Image>();
    }
    #endregion

    #region Public Method
    public virtual void OnDrag(PointerEventData pad)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImg.rectTransform, pad.position, pad.pressEventCamera, out pos))
        {
            pos.x = (pos.x / bgImg.rectTransform.sizeDelta.x);
            pos.y = (pos.y / bgImg.rectTransform.sizeDelta.y);

            inputVector = new Vector3(pos.x * 2, pos.y * 2 , 0);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;
            // Joystick Img Move
            joystickImg.rectTransform.anchoredPosition = new Vector3(inputVector.x * (bgImg.rectTransform.sizeDelta.x / 3),
                inputVector.y * (bgImg.rectTransform.sizeDelta.y / 3));

        }
    }

    public virtual void OnPointerDown(PointerEventData pad)
    {
        OnDrag(pad);
    }


    public virtual void OnPointerUp(PointerEventData pad)
    {
        inputVector = Vector3.zero;
        joystickImg.rectTransform.anchoredPosition = Vector3.zero;
    }

    public float GetHorizontalValue()
    {
        return inputVector.x;
    }

    public float GetVerticalValue()
    {
        return inputVector.y;
    }

    #endregion
}
