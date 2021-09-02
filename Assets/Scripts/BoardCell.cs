using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool _canDestroy;
    public Chessboard.Cell Cell;

    void OnEnable()
    {
        Cell = Chessboard.Instance.CurrentCell;
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
        if (!Chessboard.Instance.canPutCell) _canDestroy = false;
        
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
