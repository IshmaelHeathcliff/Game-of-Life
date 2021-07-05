using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExtendableBoardCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool _canDestroy;
    public ExtendableBoard.ExtendableCell Cell;

    void OnEnable()
    {
        Cell = ExtendableBoard.Instance.CurrentCell;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _canDestroy = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _canDestroy = false;
    }

    void Update()
    {
        if (!Cell.Status)
        {
            gameObject.SetActive(false);
            Cell.BoardCell = null;
            return;
        }
        
        if (_canDestroy && Input.GetMouseButtonUp(0))
        {
            Cell.Status = false;
            gameObject.SetActive(false);
            Cell.BoardCell = null;
        }
    }
}
