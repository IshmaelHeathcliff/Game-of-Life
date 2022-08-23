using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = System.Random;

public class Chessboard : MonoBehaviour
{
    static Chessboard _instance;
    public static Chessboard Instance {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            
            if (_instance == null)
            {
                _instance = FindObjectOfType<Chessboard>();
            }

            if (_instance == null)
            {
                var obj = new GameObject("Chessboard");
                _instance = obj.AddComponent<Chessboard>();
            }

            return _instance;
        }
    }

    [Header("Game Rules")]
    [SerializeField][Range(0, 8)] int surviveDownThreshold = 2;
    [SerializeField][Range(0, 8)] int surviveUpThreshold = 3;
    [SerializeField][Range(0, 8)] int bornDownThreshold = 3;
    [SerializeField][Range(0, 8)] int bornUpThreshold = 3;
    [SerializeField] int randomDensity = 3;
    
    [Header("Board Settings")]
    [SerializeField] float updateInterval = 1f;
    [SerializeField] GameObject updateStatusText;
    public int size  = 21;
    public int sizeLimit = 150;

    [Header("Optimization")]
    [SerializeField] int coroutineTimes = 2;
    
    [HideInInspector]public bool canPutCell = true;

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


    
    void CountNeighborhood(int x, int y)
    {
        var liveNeighbors = 0;
        for (var i = -1; i <= 1; i++)
        {
            for (var j = -1; j <= 1; j++)
            {
                if(i==0 && j==0) continue;
                var nbX = x + i;
                var nbY = y + j;
                if (nbX < 0) nbX = size - 1;
                if (nbX > size - 1) nbX = 0;
                if (nbY < 0) nbY = size - 1;
                if (nbY > size - 1) nbY = 0;
                liveNeighbors += Board[nbX, nbY].Status ? 1 : 0;
            }
        }

        // Debug.Log(liveNeighbors);
        
        if (liveNeighbors < surviveDownThreshold || liveNeighbors > surviveUpThreshold)
            Board[x, y].NextStatus = false;
        else if (Board[x, y].Status)
            Board[x, y].NextStatus = true;
        else if (liveNeighbors >= bornDownThreshold && liveNeighbors <= bornUpThreshold)
            Board[x, y].NextStatus = true;
        else
            Board[x, y].NextStatus = false;
    }

    void UpdateStatus(int x, int y)
    {
        var cell = Board[x, y];
        cell.UpdateStatus();
    }
    IEnumerator UpdateBoard()
    {
        // System.Diagnostics.Stopwatch stopwatch = new();
        // stopwatch.Start();

        for(var c=0; c < coroutineTimes; c++)
        {
            // stopwatch.Restart();
            for (var i = c; i < size; i+=coroutineTimes)
            {
                for (var j = 0; j < size; j++)
                {
                    // int x = i;
                    // int y = j;
                    // Thread t = new Thread(() => CountNeighborhood(x, y));
                    // t.Start();
                    CountNeighborhood(i, j);
                }
            }
            
            // stopwatch.Stop();
            // var neighborTime = stopwatch.Elapsed.TotalMilliseconds;
            // Debug.Log($"neighbor time: {neighborTime}");

            yield return null;
        }
        // Debug.Log("邻居计算结束");

        // 根据存活邻居更新状态
        for(var c=0; c < coroutineTimes; c++)
        {
            // stopwatch.Restart();
            for (var i = c; i < size; i+=coroutineTimes)
            {
                for (var j = 0; j < size; j++)
                {
                    // int x = i;
                    // int y = j;
                    // Thread t = new Thread(() => CountNeighborhood(x, y));
                    // t.Start();
                    UpdateStatus(i, j);
                }
            }
            
            // stopwatch.Stop();
            // var updateTime = stopwatch.Elapsed.TotalMilliseconds;
            // Debug.Log($"update time: {updateTime}");
            
            yield return null;
        }
        // Debug.Log("邻居计算结束");
        


        // stopwatch.Restart();
        // 根据倒数第三圈是否有存活Cell扩展Board
        if (!isExtendable) yield break;
        if (size < sizeLimit) ;
        {
            for (var i = 2; i < size - 2; i++)
            {
                if (!Board[i, 2].Status && !Board[i, size - 3].Status && !Board[2, i].Status &&
                    !Board[size - 3, i].Status) continue;
                ExtendTo(size / 2, size / 2);
                yield break;
            }
        }

        // stopwatch.Stop();
        // var extendTime = stopwatch.Elapsed.TotalMilliseconds;
        // Debug.Log($"extend time: {extendTime}");
    }

    void MouseInput()
    {
        if (!Input.GetMouseButtonUp(0) || !canPutCell || _cameraMove.isMoving) return;
        
        // 鼠标不在UI上
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        // Debug.Log("MouseDown");
            
        // 根据鼠标位置计算棋盘位置和矩阵位置
        var mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        var localMousePosition = transform.InverseTransformPoint(mousePosition) / (CellPool.Instance.cellSize + CellPool.Instance.cellInterval);
        var posX = Convert.ToInt32(localMousePosition.x);
        var posY = Convert.ToInt32(localMousePosition.y);
        // Debug.Log(posX);
        // Debug.Log(posY);

        // 调整矩阵状态
        if (isExtendable)
        {
            if (!ExtendTo(posX, posY)) return;
            var currentCell = Board[-posY + size / 2, posX + size / 2];
            currentCell.ChangeStatus();
        }
        else
        {
            var i = -posY + size / 2;
            var j = posX + size / 2;
            if (i < size && i >= 0 && j < size && j >= 0)
            {
                // Debug.Log($"i:{i}, j:{j}, Size:{size}");
                var currentCell = Board[i, j];
                currentCell.ChangeStatus();
            }
        }
    }
    
    bool ExtendTo(int x, int y)
    {
        y = Math.Abs(y);
        x = Math.Abs(x);

        if (sizeLimit < 2 * y + 1 || sizeLimit < 2 * x + 1) return false;
        if (size > 4 * y + 1 && size > 4 * x + 1) return true;
        
        var preSize = size;
        
        var newSize1 = Math.Min(2 * size + 1, sizeLimit);
        var newSize2 = Math.Max(Math.Min(sizeLimit, 4 * y + 1), Math.Min(sizeLimit, 4 * x + 1));
        size = Math.Max(newSize1, newSize2);


        var newBoard = new CellPool.Cell[size, size];
        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                var offset = (size - preSize) / 2;
                if (i >= offset && i <= offset + preSize - 1 &&
                    j >= offset && j <= offset + preSize - 1)
                {
                    newBoard[i, j] = Board[i - offset, j - offset];
                }
                else
                {
                    newBoard[i, j] = _pool.GetCell();
                    newBoard[i, j].SetPos(i, j, transform);
                }
            }
        }
        Board = newBoard;
        return true;
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
    
    public void InitialiseBoard()
    {
        if (Board != null)
        {
            foreach (var cell in Board)
            {
               _pool.AddCell(cell); 
            }
        }
        
        Board = new CellPool.Cell[size, size];
        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                Board[i, j] = _pool.GetCell();
                Board[i, j].SetPos(i, j, transform);
            }
        }
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
        sizeLimit = sizeLimit / 2 * 2 + 1;
    }

    void Start()
    {
        
        _nextUpdateTime = updateInterval;
        _cameraMove = _camera.GetComponent<CameraMove>();
        InitialiseBoard();
    }

    void Update()
    {
        // System.Diagnostics.Stopwatch stopwatch = new();
        // stopwatch.Start();
        if (_canUpdate && !_isContinuousUpdate)
        {
            StartCoroutine(UpdateBoard());
            _canUpdate = false;
        }

        if (_canUpdate && _isContinuousUpdate)
        {
            if (_nextUpdateTime < 0)
            {
                StartCoroutine(UpdateBoard());
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
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    Board[i, j].Disable();
                }
            }

            StopUpdate();
            _isClear = false;
        }
    }
}