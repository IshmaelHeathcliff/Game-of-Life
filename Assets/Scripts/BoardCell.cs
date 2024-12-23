using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class BoardCell : MonoBehaviour //, IPointerEnterHandler, IPointerExitHandler
{
    // public bool CanChange { get; private set; }
    SpriteRenderer _renderer;

    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     CanChange = true;
    // }
    //
    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     CanChange = false;
    // }

    public void Disable()
    {
        var c = _renderer.color;
        _renderer.color = new Color(c.r, c.g, c.b, 0.2f);
    }

    public void Enable()
    {
        var c = _renderer.color;
        _renderer.color = new Color(c.r, c.g, c.b, 1);
    }

    public void Hide()
    {
        var c = _renderer.color;
        _renderer.color = new Color(c.r, c.g, c.b, 0);
    }

    // public int[] BoardPosition()
    // {
    //     int boardRows = Chessboard.Instance.Rows;
    //     int boardColumns = Chessboard.Instance.Columns;
    //     Vector3 localPosition = transform.localPosition;
    //     int i = -Convert.ToInt32(localPosition.y) + boardRows / 2;
    //     int j = Convert.ToInt32(localPosition.x) + boardColumns / 2;
    //     return new[] {i, j};
    // }

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    // void Update()
    // {
        // if (!Chessboard.Instance.canPutCell) _canChange = false;
        //
        // if (!_canChange || !Input.GetMouseButtonUp(0)) return;
        //
        // int[] boardPosition = BoardPosition();
        // int i = boardPosition[0];
        // int j = boardPosition[1];
        // Chessboard.Cell currentCell = Chessboard.Instance.Board[i, j];
        // if (currentCell.Status)
        // {
        //     currentCell.Hide();
        // }
        // else
        // {
        //     currentCell.Display();
        // }
    // }
}
