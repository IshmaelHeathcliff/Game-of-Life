using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Background : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        ExtendableBoard.Instance.canPutCell = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ExtendableBoard.Instance.canPutCell = false;
    }
}
