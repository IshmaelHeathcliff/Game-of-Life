using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardCell : MonoBehaviour //, IPointerEnterHandler, IPointerExitHandler
{
    bool _canChange;
    SpriteRenderer _renderer;

    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     _canChange = true;
    // }
    //
    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     _canChange = false;
    // }

    public void Hide()
    {
        Color c = _renderer.color;
        _renderer.color = new Color(c.r, c.g, c.b, 0.2f);
    }

    public void Display()
    {
        Color c = _renderer.color;
        _renderer.color = new Color(c.r, c.g, c.b, 1);
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

    void Update()
    {
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
    }
}
