using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExtendableBoardCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool _canDestroy;
    public ExtendableBoard.ExtendableCell Cell;

    void Awake()
    {
        Cell = ExtendableBoard.Instance.CurrentCell;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Cell.Status)
        {
            DestroyImmediate(gameObject);
            DestroyImmediate(this);
        }
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
        if (_canDestroy && Input.GetMouseButtonUp(0))
        {
            Cell.Status = false;
            DestroyImmediate(gameObject);
            DestroyImmediate(this);
        }
    }
}
