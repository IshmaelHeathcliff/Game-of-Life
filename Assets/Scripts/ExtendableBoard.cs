using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExtendableBoard : MonoBehaviour
{
    static ExtendableBoard _instance;
    public static ExtendableBoard Instance {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            
            if (_instance == null)
            {
                _instance = FindObjectOfType<ExtendableBoard>();
            }

            if (_instance == null)
            {
                var obj = new GameObject("Chessboard");
                _instance = obj.AddComponent<ExtendableBoard>();
            }

            return _instance;
        }
    }

    [SerializeField][Range(0, 8)] int liveUpThreshold = 3;
    [SerializeField][Range(0, 8)] int liveDownThreshold = 2;
    [SerializeField][Range(0, 8)] int bornUpThreshold = 3;
    [SerializeField][Range(0, 8)] int bornDownThreshold = 3;
    [SerializeField] float cellInterval = 0.1f;
    [SerializeField] float cellSize = 0.5f;
    [SerializeField] float updateInterval = 1f;

    [SerializeField] GameObject boardCell;
    [SerializeField] GameObject boardHolder; // 棋盘格子放在这个GameObject下
    [SerializeField] GameObject updateStatusText;

    bool _canUpdate;
    bool _isContinuousUpdate;
    float _nextUpdateTime;
    Camera _camera;
    BoardHolderMovement _boardHolderMovement;
    Text _updateStatusText;

    [HideInInspector] public bool canPutCell = true;
    public ExtendableCell CurrentCell;
    
    public class ExtendableCell
    {
        public bool Status;
        public int LiveNeighbours;
        public GameObject BoardCell;

        public ExtendableCell()
        {
            Status = false;
            LiveNeighbours = 0;
        }
    }
    
    // _matrix每行如下所示对应一个棋盘
    // <-7 6-->
    // <-3 2-->
    // <-1 0-->
    // <-5 4-->
    // <-9 8-->
    List<List<ExtendableCell>> _matrix;
    int _matrixColumns;

    // 解决棋盘前两行的特殊相邻行问题
    readonly int[] _sp = new[] {1, 0, 3, 2};

    void Initialise(int rows=6, int columns=2)
    {
        _matrix = new List<List<ExtendableCell>>();
        _matrixColumns = columns;
        ExtendMatRows(rows);
    }

    void ExtendMatRows(int row=1)
    {
        for (var i = 0; i < row; i++)
        {        
            var list = new List<ExtendableCell>();
            for(var k=0; k<_matrixColumns;k++) list.Add(new ExtendableCell());
            _matrix.Add(list);
        }
    }

    void ExtendMatColumns(int column=1)
    {
        if (column % 2 == 1) column++;
        foreach (var list in _matrix)
        {
            for (var i = 0; i < column; i++)
            {
                list.Add(new ExtendableCell());
            }
        }
        _matrixColumns += column;
    }

    void ExtendTo(int x, int y)
    {
        x += 5;
        y += 2;
        if (_matrix.Count < x)
        {
            ExtendMatRows(x -_matrix.Count);
        }

        if (_matrixColumns < y)
        {
            ExtendMatColumns(y - _matrixColumns);
        }
    }

    static Vector3 MatrixPositionToBoardPosition(int matRow, int matColumn)
    {
        int boardRow = matRow / 2 % 2 == 0 ? -matRow / 2 / 2 : matRow / 2 / 2 + 1;
        int boardColumn = matRow % 2 == 0 ? matColumn : -matColumn - 1;
        return new Vector3(boardColumn, boardRow, 0);
    }

    static int[] BoardPositionToMatrixPosition(int[] boardPosition)
    {
        int x = boardPosition[0];
        int y = boardPosition[1];
        int offset = x < 0 ? 1 : 0;
        int matRow = y > 0 ? 4 * y - 2 + offset : -y * 4 + offset;
        int matColumn = x < 0 ? -x - 1 : x;
        return new []{matRow, matColumn};
    }

    void BoardUpdate()
    {
        // 根据存活cell计算存活邻居
        for (var i = 0; i < _matrix.Count; i++)
        {
            for (var j = 0; j < _matrixColumns; j++)
            {
                if (!_matrix[i][j].Status) continue;
                for (int r = -1; r < 2; r++)
                {
                    for (int c = -1; c < 2; c++)
                    {
                        if (r == 0 && c == 0) continue;
                        int rr = 4*r + i;
                        int cc = c + j;
                        
                        // 解决棋盘前两行的特殊相邻行问题
                        if (rr < 0) rr = _sp[-rr - 1]; 
                            
                        // 棋盘相邻列在矩阵中的跨行问题
                        if (cc < 0)
                        {
                            cc = 0;
                            rr = rr % 2 == 0 ? rr + 1 : rr - 1;
                        }
                        
                        if (rr < _matrix.Count && cc < _matrixColumns)
                        {
                            _matrix[rr][cc].LiveNeighbours++;
                        }
                    }
                }
            }
        }
        
        // 根据存活邻居数决定新的状态
        foreach (ExtendableCell cell in _matrix.SelectMany(row => row))
        {
            if (cell.LiveNeighbours < liveDownThreshold || cell.LiveNeighbours > liveUpThreshold)
            {
                cell.Status = false;
            }
            else
            {
                if (cell.Status)
                {
                    cell.Status = true;
                }
                else if (cell.LiveNeighbours >= bornDownThreshold && cell.LiveNeighbours <= bornUpThreshold)
                {
                    cell.Status = true;
                }
            }

            // 计算完状态重置存活邻居数
            cell.LiveNeighbours = 0;
        }
        
        // 根据状态更新棋盘
        for (var i = 0; i < _matrix.Count; i++)
        {
            for (var j = 0; j < _matrixColumns; j++)
            {
                CurrentCell = _matrix[i][j];
                if (!CurrentCell.Status) continue;
                ExtendTo(i, j);
                if (CurrentCell.BoardCell == null)
                {
                    Vector3 boardPosition = MatrixPositionToBoardPosition(i, j);
                    GameObject cell = Instantiate(boardCell, boardHolder.transform);
                    cell.transform.localPosition = boardPosition * (cellSize + cellInterval);
                    cell.name = $"BoardCell{boardPosition.y}, {boardPosition.x}";
                    CurrentCell.BoardCell = cell;
                }
            }
        }
        
    }
    
    public void UpdateOnce()
    {
        _canUpdate = true;
        _isContinuousUpdate = false;
        _updateStatusText.text = "Finished";
    }

    public void StartUpdate()
    {
        _isContinuousUpdate = true;
        _canUpdate = true;
        _updateStatusText.text = "On";
    }

    public void StopUpdate()
    {
        _canUpdate = false;
        _isContinuousUpdate = false;
        _updateStatusText.text = "Off";
    }

    public void ClearBoard()
    {
        StopUpdate();
        foreach (ExtendableCell cell in _matrix.SelectMany(row => row))
        {
            cell.Status = false;
            cell.LiveNeighbours = 0;
        }
    }

    void Awake()
    {
        if (Instance != this)
        {
            DestroyImmediate(this);
            return;
        }
        
        Initialise();
        _camera = Camera.main;
        _updateStatusText = updateStatusText.GetComponent<Text>();
    }

    void Start()
    {
        boardCell.transform.localScale = new Vector3(cellSize, cellSize, 1);
        _nextUpdateTime = updateInterval;

        _boardHolderMovement = boardHolder.GetComponent<BoardHolderMovement>();

        // init chessboard
        if (boardHolder == null)
            boardHolder = GameObject.Find("Board");
        if (boardHolder == null)
        {
            boardHolder = new GameObject("Board");
            boardHolder.AddComponent<BoardHolderMovement>();
        }

        // foreach (Cell cell in _matrix.SelectMany(row => row)) Debug.Log(cell.Status);
    }
    
    void FixedUpdate()
    {
        if (_canUpdate && !_isContinuousUpdate)
        {
            BoardUpdate();
            _canUpdate = false;
        }

        if (_canUpdate && _isContinuousUpdate)
        {
            if (_nextUpdateTime < 0)
            {
                BoardUpdate();
                _nextUpdateTime = updateInterval;
            }
            else
            {
                _nextUpdateTime -= Time.fixedDeltaTime;
            }
        }
        
    }

    void Update()
    {
        if (canPutCell && !_boardHolderMovement.isBoardMoving && Input.GetMouseButtonUp(0))
        {
            // 根据鼠标位置计算棋盘位置和矩阵位置
            Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 localMousePosition =
                boardHolder.transform.InverseTransformPoint(mousePosition) / (cellSize + cellInterval);
            var boardPosition = new[] {Convert.ToInt32(localMousePosition.x), Convert.ToInt32(localMousePosition.y)};
            int[] matrixPosition = BoardPositionToMatrixPosition(boardPosition);

            
            // 调整矩阵状态
            ExtendTo(matrixPosition[0], matrixPosition[1]);
            CurrentCell = _matrix[matrixPosition[0]][matrixPosition[1]];
            CurrentCell.Status = true;
            
            // 实例化棋盘单元
            GameObject cell = Instantiate(boardCell, boardHolder.transform);
            cell.transform.localPosition = new Vector3(boardPosition[0], boardPosition[1], 0) * (cellSize + cellInterval);
            cell.name = $"BoardCell{boardPosition[1]}, {boardPosition[0]}";
            CurrentCell.BoardCell = cell;
        }
    }
}
