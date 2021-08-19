using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChessBoard : MonoBehaviour
{
    static ChessBoard _instance;
    public static ChessBoard Instance {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            
            if (_instance == null)
            {
                _instance = FindObjectOfType<ChessBoard>();
            }

            if (_instance == null)
            {
                var obj = new GameObject("Chessboard");
                _instance = obj.AddComponent<ChessBoard>();
            }

            return _instance;
        }
    }

    [Header("Game Rules")]
    [SerializeField][Range(0, 8)] int liveUpThreshold = 3;
    [SerializeField][Range(0, 8)] int liveDownThreshold = 2;
    [SerializeField][Range(0, 8)] int bornUpThreshold = 3;
    [SerializeField][Range(0, 8)] int bornDownThreshold = 3;
    
    [Header("Board Settings")]
    [SerializeField] float cellInterval = 0.1f;
    [SerializeField] float cellSize = 0.5f;
    [SerializeField] float updateInterval = 1f;
    [SerializeField] GameObject boardCell;
    [SerializeField] GameObject boardHolder; // 棋盘格子放在这个GameObject下
    [SerializeField] GameObject updateStatusText;

    bool _canUpdate;
    [HideInInspector] public bool canPutCell = true;
    bool _isContinuousUpdate;
    float _nextUpdateTime;
    Camera _camera;
    BoardHolderMovement _boardHolderMovement;
    Text _updateStatusText;

    BoardCellPool pool;
    public Cell CurrentCell;
    
    public class Cell
    {
        public bool Status;
        public int LiveNeighbours;
        public GameObject BoardCell;

        public Cell()
        {
            Status = false;
            LiveNeighbours = 0;
        }
    }
    
    // 列向量
    ExtendableMatrix<Cell> _board;

    void BoardUpdate()
    {
        // 根据存活cell计算存活邻居
        for (var i = 0; i < _board.Columns; i++)
        {
            for (var j = 0; j < _board.Rows; j++)
            {
                if (!_board.Matrix[i][j].Status) continue;
                for (int r = -1; r < 2; r++)
                {
                    for (int c = -1; c < 2; c++)
                    {
                        if (r == 0 && c == 0) continue;
                        int rr = r + i;
                        int cc = c + j;

                        if (rr < _board.Columns && rr >= 0  && cc < _board.Rows && cc >= 0)
                        {
                            _board.Matrix[rr][cc].LiveNeighbours++;
                        }
                    }
                }
            }
        }
        
        // 根据存活邻居数决定新的状态
        foreach (Cell cell in _board.Matrix.SelectMany(row => row))
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
        for (var i = 0; i < _board.Columns; i++)
        {
            for (var j = 0; j < _board.Rows; j++)
            {
                CurrentCell = _board.Matrix[i][j];
                if (!CurrentCell.Status) continue;
                if (CurrentCell.BoardCell == null)
                {
                    var boardPosition = new Vector3(i - _board.Columns / 2, j - _board.Rows / 2);
                    GameObject cell = pool.GetCell();
                    cell.transform.localPosition = boardPosition * (cellSize + cellInterval);
                    cell.name = $"BoardCell{boardPosition.x}, {boardPosition.y}";
                    CurrentCell.BoardCell = cell;
                }
                _board.ExtendTo(i - _board.Columns/2, j - _board.Rows/2);
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
        canPutCell = false;
    }

    public void StopUpdate()
    {
        _canUpdate = false;
        _isContinuousUpdate = false;
        _updateStatusText.text = "Off";
        canPutCell = true;
    }

    public void Clear()
    {
        StartCoroutine($"ClearBoard");
    }

    IEnumerator ClearBoard()
    {
        StopUpdate();
        yield return null;
        foreach (Cell cell in _board.Matrix.SelectMany(row => row))
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

        _board = new ExtendableMatrix<Cell>();
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
            boardHolder = GameObject.Find("BoardPool");
        if (boardHolder == null)
        {
            boardHolder = new GameObject("BoardPool");
            boardHolder.AddComponent<BoardHolderMovement>();
            boardHolder.AddComponent<BoardCellPool>();
        }

        pool = boardHolder.GetComponent<BoardCellPool>();
        pool.boardCell = boardCell;
    }

    void Update()
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
                _nextUpdateTime -= Time.deltaTime;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            UpdateOnce();
        }
        
        if (Input.GetMouseButtonUp(0) && canPutCell && !_boardHolderMovement.isBoardMoving)
        {
            // 鼠标不在UI上
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            // 根据鼠标位置计算棋盘位置和矩阵位置
            Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 localMousePosition =
                boardHolder.transform.InverseTransformPoint(mousePosition) / (cellSize + cellInterval);
            var boardX = Convert.ToInt32(localMousePosition.x);
            var boardY = Convert.ToInt32(localMousePosition.y);
            
            // 调整矩阵状态
            _board.ExtendTo(boardX, boardY);
            CurrentCell = _board.Matrix[boardX + _board.Columns/2][boardY + _board.Rows/2];
            CurrentCell.Status = true;
            
            // 实例化棋盘单元
            GameObject cell = pool.GetCell();
            cell.transform.localPosition = new Vector3(boardX, boardY, 0) * (cellSize + cellInterval);
            cell.name = $"BoardCell{boardX}, {boardY}";
            CurrentCell.BoardCell = cell;
        }
    }
}
