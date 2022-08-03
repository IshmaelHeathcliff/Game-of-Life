using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public class Chessboard
{
    public Chessboard(int row = 21, int column = 21, 
        int surviveDownThreshold = 2, int surviveUpThreshold = 3,
        int bornDownThreshold = 3, int bornUpThreshold = 3)
    {
        Rows = row;
        Columns = column;
        _surviveDownThreshold = surviveDownThreshold;
        _surviveUpThreshold = surviveUpThreshold;
        _bornDownThreshold = bornDownThreshold;
        _bornUpThreshold = bornUpThreshold;
        Status = new bool[Rows, Columns];
        _nextBoard = new bool[Rows, Columns];
    }

    readonly int _surviveUpThreshold;
    readonly int _surviveDownThreshold;
    readonly int _bornUpThreshold;
    readonly int _bornDownThreshold;
    bool[,] _nextBoard;

    public int Rows { get; private set; }
    public int Columns { get; private set; }

    public bool[,] Status { get; private set; }

    int CountNeighborhood(int x, int y)
    {
        int liveNeighbors = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if(i==0 && j==0) continue;
                liveNeighbors += Status[x + i, y + j] ? 1 : 0;
            }
        }

        return liveNeighbors;
    }

    public void UpdateBoard()
    {
        for(int i = 1; i < Rows - 1; i++)
        {
            for (int j = 1; j < Columns - 1; j++)
            {
                int liveNeighbors = CountNeighborhood(i, j);
                if (liveNeighbors < _surviveDownThreshold || liveNeighbors > _surviveUpThreshold)
                    _nextBoard[i, j] = false;
                else if (Status[i, j])
                    _nextBoard[i, j] = true;
                else if (liveNeighbors >= _bornDownThreshold && liveNeighbors <= _bornUpThreshold)
                    _nextBoard[i, j] = true;
                else _nextBoard[i, j] = false;
            }
        }

        (Status, _nextBoard) = (_nextBoard, Status);
    }

    public void ExtendTo(int x, int y)
    {
        y = Math.Abs(y);
        x = Math.Abs(x);
        if (Rows > 4 * y + 1 && Columns > 4 * x + 1) return;
        int newRows = 2 * Rows + 1;
        int newColumns = 2 * Columns + 1;
        if(4 * y + 1 > newRows) newRows = 4 * y + 1;
        if(4 * x + 1 > newColumns) newColumns = 4 * x + 1;
        bool[,] newBoard = new bool[newRows, newColumns];
        for(int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if(Status[i, j])
                    newBoard[(newRows - Rows) / 2 + i, (newColumns - Columns) / 2 + j] = true;
            }
        }

        Rows = newRows;
        Columns = newColumns;
        Status = newBoard;
        _nextBoard = new bool[Rows, Columns];
    }

    public void Clear()
    {
        Status = new bool[Rows, Columns];
    }
}
