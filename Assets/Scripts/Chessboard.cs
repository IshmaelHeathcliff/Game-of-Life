using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = System.Random;

public class Chessboard : MonoBehaviour
{
    static Chessboard _instance;

    public static Chessboard Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            if (_instance == null)
            {
                _instance = FindAnyObjectByType<Chessboard>();
            }

            if (_instance == null)
            {
                var obj = new GameObject("Chessboard");
                _instance = obj.AddComponent<Chessboard>();
            }

            return _instance;
        }
    }

    [Header("Game Rules")] [SerializeField] [Range(0, 8)]
    int surviveDownThreshold = 2;

    [SerializeField] [Range(0, 8)] int surviveUpThreshold = 3;
    [SerializeField] [Range(0, 8)] int bornDownThreshold = 3;
    [SerializeField] [Range(0, 8)] int bornUpThreshold = 3;
    [SerializeField] int randomDensity = 3;

    [Header("Board Settings")] [SerializeField][Range(0.1f, 1)]
    float updateInterval = 1f;

    [SerializeField] GameObject updateStatusText;

    [Header("Optimization")] [SerializeField]
    int coroutineTimes = 2;

    [HideInInspector] public bool canPutCell = true;

    bool _canUpdate;
    public bool isExtendable;
    bool _isContinuousUpdate;
    bool _isClear;
    float _nextUpdateTime;
    Camera _camera;
    CameraMove _cameraMove;
    Text _updateStatusText;
    CellPool _pool;

    CellPool.Cell[,] Board { get; set; }
    Dictionary<Vector2Int, CellPool.Cell> grid = new();

    int CountLiveNeighbors(Vector2Int pos)
    {
        var count = 0;
        for (var i = -1; i <= 1; i++)
        {
            for (var j = -1; j <= 1; j++)
            {
                var neighborPos = new Vector2Int(pos.x + i, pos.y + j);
                if (neighborPos != pos && grid.ContainsKey(neighborPos) &&
                    grid[neighborPos].Status)
                {
                    count++;
                }
            }
        }

        return count;
    }


    void UpdateStatus(Vector2Int pos)
    {
        var cell = grid[pos];
        cell.UpdateStatus();
    }

    void UpdateGrid()
    {
        var newGrid = new Dictionary<Vector2Int, bool>();
        var cellsToCheck = new HashSet<Vector2Int>(grid.Keys);
        foreach (var pos in grid.Keys)
        {
            for (var i = -1; i <= 1; i++)
            {
                for (var j = -1; j <= 1; j++)
                {
                    var neighborPos = new Vector2Int(pos.x + i, pos.y + j);
                    cellsToCheck.Add(neighborPos);
                }
            }
        }

        foreach (var pos in cellsToCheck)
        {
            var liveNeighbors = CountLiveNeighbors(pos);
            var isAlive = grid.ContainsKey(pos) && grid[pos].Status;
            if (isAlive && (liveNeighbors < surviveDownThreshold || liveNeighbors > surviveUpThreshold))
            {
                newGrid[pos] = false;
            }
            else if (!isAlive && (liveNeighbors >= bornDownThreshold && liveNeighbors <= bornUpThreshold))
            {
                newGrid[pos] = true;
            }
            else
            {
                newGrid[pos] = isAlive;
            }
        }

        foreach (var item in newGrid)
        {
            if (item.Value)
            {
                if (!grid.ContainsKey(item.Key))
                {
                    var pos = item.Key;
                    var cell = _pool.Pop();
                    cell.Status = true;
                    grid[item.Key] = cell;
                }

                grid[item.Key].SetPos(item.Key, transform);
            }
            else
            {
                if (grid.ContainsKey(item.Key))
                {
                    _pool.Push(grid[item.Key]);
                    grid.Remove(item.Key);
                }
            }
        }
    }

    void MouseInput()
    {
        if (!Input.GetMouseButtonUp(0) || !canPutCell || _cameraMove.isMoving) return;

        // 鼠标不在UI上
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Debug.Log("MouseDown");

        // 根据鼠标位置计算棋盘位置和矩阵位置
        var mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        var localMousePosition = transform.InverseTransformPoint(mousePosition) /
                                 (CellPool.Instance.cellSize + CellPool.Instance.cellInterval);
        var posX = Convert.ToInt32(localMousePosition.x);
        var posY = Convert.ToInt32(localMousePosition.y);
        var pos = new Vector2Int(posX, posY);
        // Debug.Log(posX);
        // Debug.Log(posY);

        if (!grid.ContainsKey(pos))
        {
            grid[pos] = _pool.Pop();
            grid[pos].SetPos(pos, transform);
        }
        else
        {
            _pool.Push(grid[pos]);
            grid.Remove(pos);
        }
    }

    public void Clear()
    {
        _isClear = true;
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

    public void RandomiseBoard()
    {
        var rd = new Random();
        foreach (var cell in Board)
        {
            cell.NextStatus = rd.Next(randomDensity) < 1;
            cell.UpdateStatus();
        }
    }

    void Awake()
    {
        if (Instance != this)
        {
            DestroyImmediate(this);
            return;
        }

        Application.targetFrameRate = 60;

        _camera = Camera.main;
        _updateStatusText = updateStatusText.GetComponent<Text>();
        _pool = CellPool.Instance;
    }

    void Start()
    {
        
        _nextUpdateTime = updateInterval;
        _cameraMove = _camera.GetComponent<CameraMove>();
    }

    void Update()
    {
        // System.Diagnostics.Stopwatch stopwatch = new();
        // stopwatch.Start();
        if (_canUpdate && !_isContinuousUpdate)
        {
            UpdateGrid();
            _canUpdate = false;
        }

        if (_canUpdate && _isContinuousUpdate)
        {
            if (_nextUpdateTime < 0)
            {
                UpdateGrid();
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
        
        MouseInput();

        // Debug.Log(transform.childCount);
        // stopwatch.Stop();
        // double updateTime = stopwatch.Elapsed.TotalMilliseconds;
        // if(updateTime > 1)
        //     Debug.Log($"update time: {updateTime}");
    }

    void LateUpdate()
    {
        if (_isClear)
        {
            foreach (var item in grid)
            {
                _pool.Push(item.Value);
            }

            grid = new Dictionary<Vector2Int, CellPool.Cell>();
            StopUpdate();
            _isClear = false;
        }
    }
}