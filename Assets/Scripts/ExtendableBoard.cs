using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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
    bool _canPutCell = true;
    bool _isContinuousUpdate;
    float _nextUpdateTime;
    Camera _camera;
    BoardHolderMovement _boardHolderMovement;
    Text _updateStatusText;

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
    
    // 列向量
    List<List<ExtendableCell>> _matrix;
    int _matrixRows;

    void Initialise(int rows=11, int columns=11)
    {
        _matrix = new List<List<ExtendableCell>>();
        _matrixRows = rows;
        ExtendMatColumns(columns);
    }

    void ExtendMatRows(int rowPair=1)
    {
        foreach (var list in _matrix)
        {
            for (var i = 0; i < rowPair; i++)
            {
                list.Add(new ExtendableCell());
                list.Insert(0, new ExtendableCell());
            }
        }
        _matrixRows += 2*rowPair;

    }

    void ExtendMatColumns(int columnPair=1)
    {
        for (var i = 0; i < columnPair; i++)
        {        
            var list1 = new List<ExtendableCell>();
            var list2 = new List<ExtendableCell>();
            for (var k = 0; k < _matrixRows; k++)
            {
                list1.Add(new ExtendableCell());
                list2.Add(new ExtendableCell());
            }
            _matrix.Add(list1);
            _matrix.Insert(0, list2);
        }

    }

    void ExtendTo(int x, int y)
    {
        x = Math.Abs(x) + 1;
        y = Math.Abs(y) + 1;
        if (_matrix.Count < 2 * x + 1)
        {
            ExtendMatColumns(x < _matrix.Count? _matrix.Count/2 : x);
        }

        if (_matrixRows < 2 * y + 1)
        {
            ExtendMatRows(y < _matrixRows? _matrixRows/2 : y);
        }
    }

    void BoardUpdate()
    {
        // 根据存活cell计算存活邻居
        for (var i = 0; i < _matrix.Count; i++)
        {
            for (var j = 0; j < _matrixRows; j++)
            {
                if (!_matrix[i][j].Status) continue;
                for (int r = -1; r < 2; r++)
                {
                    for (int c = -1; c < 2; c++)
                    {
                        if (r == 0 && c == 0) continue;
                        int rr = r + i;
                        int cc = c + j;

                        if (rr < _matrix.Count && rr >= 0  && cc < _matrixRows && cc >= 0)
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
            for (var j = 0; j < _matrixRows; j++)
            {
                CurrentCell = _matrix[i][j];
                if (!CurrentCell.Status) continue;
                ExtendTo(i - _matrix.Count/2, j - _matrixRows/2);
                if (CurrentCell.BoardCell == null)
                {
                    var boardPosition = new Vector3(i - _matrix.Count / 2, j - _matrixRows / 2);
                    GameObject cell = Instantiate(boardCell, boardHolder.transform);
                    cell.transform.localPosition = boardPosition * (cellSize + cellInterval);
                    cell.name = $"BoardCell{boardPosition.x}, {boardPosition.y}";
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
        _canPutCell = false;
    }

    public void StopUpdate()
    {
        _canUpdate = false;
        _isContinuousUpdate = false;
        _updateStatusText.text = "Off";
        _canPutCell = true;
    }

    public void Clear()
    {
        StartCoroutine($"ClearBoard");
    }

    IEnumerator ClearBoard()
    {
        StopUpdate();
        yield return null;
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
        
        if (_canPutCell && Input.GetMouseButtonUp(0) && !_boardHolderMovement.isBoardMoving)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
                // 根据鼠标位置计算棋盘位置和矩阵位置
            Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 localMousePosition =
                boardHolder.transform.InverseTransformPoint(mousePosition) / (cellSize + cellInterval);
            var boardX = Convert.ToInt32(localMousePosition.x);
            var boardY = Convert.ToInt32(localMousePosition.y);
            
            // 调整矩阵状态
            ExtendTo(boardX, boardY);
            CurrentCell = _matrix[boardX + _matrix.Count/2][boardY + _matrixRows/2];
            CurrentCell.Status = true;
            
            // 实例化棋盘单元
            GameObject cell = Instantiate(boardCell, boardHolder.transform);
            cell.transform.localPosition = new Vector3(boardX, boardY, 0) * (cellSize + cellInterval);
            cell.name = $"BoardCell{boardX}, {boardY}";
            CurrentCell.BoardCell = cell;
        }
    }
}
