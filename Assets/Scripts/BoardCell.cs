using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool _canDestroy;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _canDestroy = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _canDestroy = false;
    }

    int[] BoardPosition()
    {
        int[] boardSize = ChessboardController.Instance.GetBoardSize();
        Vector3 localPosition = transform.localPosition;
        int i = Convert.ToInt32(localPosition.y) + boardSize[0] / 2;
        int j = Convert.ToInt32(localPosition.x) + boardSize[1] / 2;
        return new[] {i, j};
    }

    void Update()
    {
        if (!ChessboardController.Instance.canPutCell) _canDestroy = false;

        if (!_canDestroy || !Input.GetMouseButtonUp(0)) return;
        int[] pos = BoardPosition();
        int i = pos[0];
        int j = pos[1];
        ChessboardController.Instance.Board.Status[i, j] = false;
        gameObject.SetActive(false);
    }
}
