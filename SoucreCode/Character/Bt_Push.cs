using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Bt_Push : MonoBehaviour, IPointerDownHandler
{
    public PlayerMove PlayerMove;
   
    #region Public Method
    public virtual void OnPointerDown(PointerEventData pad)
    {
        PlayerMove.push();
    }
    #endregion
}
