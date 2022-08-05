using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
                GameObject obj = new GameObject("Chessboard");
                _instance = obj.AddComponent<Chessboard>();
                obj.AddComponent<BoardCellPool>();
            }

            return _instance;
        }
    }
    public class Cell
    {
        readonly GameObject _cellObject;
        readonly BoardCell _boardCell;
        public bool Status;
        public bool NextStatus;

        public Cell()
        {
            _cellObject = Instance.pool.GetCell();
            _boardCell = _cellObject.GetComponent<BoardCell>();
            
            Status = false;
            _boardCell.Hide();
        }

        public void SetPos(int i, int j)
        {
            Vector3 pos = new(j - Instance.Size / 2, Instance.Size / 2 - i);
            _cellObject.name = $"BoardCell{pos.x}, {pos.y}";
            _cellObject.transform.localPosition = pos * (Instance.cellSize + Instance.cellInterval);
        }

        public void Display()
        {
            Status = true;
            _boardCell.Display();
        }

        public void Hide()
        {
            Status = false;
            _boardCell.Hide();
        }
    }

    public GameObject cellObject;
    

    [Header("Game Rules")]
    [SerializeField][Range(0, 8)] int surviveDownThreshold = 2;
    [SerializeField][Range(0, 8)] int surviveUpThreshold = 3;
    [SerializeField][Range(0, 8)] int bornDownThreshold = 3;
    [SerializeField][Range(0, 8)] int bornUpThreshold = 3;
    
    [Header("Board Settings")]
    [SerializeField] float cellInterval = 0f;
    [SerializeField] float cellSize = 0.5f;
    [SerializeField] float updateInterval = 1f;
    [SerializeField] GameObject updateStatusText;
    [SerializeField] int sizeLimit = 20;
    
    bool _canUpdate;
    public bool canPutCell = true;
    bool _isContinuousUpdate;
    float _nextUpdateTime;
    Camera _camera;
    CameraMove _cameraMove;
    Text _updateStatusText;
    public BoardCellPool pool;

    public int Size { get; private set; } = 21;
    public Cell[,] Board { get; private set; }
    
    
    int CountNeighborhood(int x, int y)
    {
        int liveNeighbors = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if(i==0 && j==0) continue;
                int nbX = x + i;
                int nbY = y + j;
                if (nbX < 0) nbX = Size - 1;
                if (nbX > Size - 1) nbX = 0;
                if (nbY < 0) nbY = Size - 1;
                if (nbY > Size - 1) nbY = 0;
                liveNeighbors += Board[nbX, nbY].Status ? 1 : 0;
            }
        }

        return liveNeighbors;
    }

    void UpdateBoard()
    {
        // System.Diagnostics.Stopwatch stopwatch = new();
        // stopwatch.Start();

        // 计算存活邻居
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                int liveNeighbors = CountNeighborhood(i, j);
                if (liveNeighbors < surviveDownThreshold || liveNeighbors > surviveUpThreshold)
                    Board[i, j].NextStatus = false;
                else if (Board[i, j].Status)
                    Board[i, j].NextStatus = true;
                else if (liveNeighbors >= bornDownThreshold && liveNeighbors <= bornUpThreshold)
                    Board[i, j].NextStatus = true;
                else
                    Board[i, j].NextStatus = false;
            }
        }

        // 根据存活邻居更新状态
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Cell cell = Board[i, j];
                if (cell.NextStatus)
                {
                    cell.Display();
                }
                else
                {
                    cell.Hide();
                }
            }
        }

        // 根据倒数第三圈是否有存活Cell扩展Board
        if (Size < sizeLimit) ;
        {
            for (int i = 2; i < Size - 2; i++)
            {
                if (Board[i, 2].Status || Board[i, Size - 3].Status || Board[2, i].Status || Board[Size - 3, i].Status)
                {
                    ExtendTo(Size / 2, Size / 2);
                }
            }
        }

        // stopwatch.Stop();
        // double updateTime = stopwatch.Elapsed.TotalMilliseconds;
        // Debug.Log($"update time: {updateTime}");
    }

    public void ExtendTo(int x, int y)
    {
        y = Math.Abs(y);
        x = Math.Abs(x);
        if (Size > 2 * y + 1 && Size > 2 * x + 1) return;
        if (Size >= sizeLimit) return;
        int preSize = Size;
        Size = 2 * Size + 1;
        if (4 * y + 1 > Size || 4 * x + 1 > Size)
        {
            Size = x > y? 4 * x + 1 : 4 * y + 1;
        }
        var newBoard = new Cell[Size, Size];
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                int offset = (Size - preSize) / 2;
                if (i >= offset && i <= offset + preSize - 1 &&
                    j >= offset && j <= offset + preSize - 1)
                {
                    newBoard[i, j] = Board[i - offset, j - offset];
                }
                else
                {
                    newBoard[i, j] = new Cell();
                    newBoard[i, j].SetPos(i, j);
                }
            }
        }
        Board = newBoard;
    }

    public void Clear()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Board[i, j].Hide();
            }
        }
        StopUpdate();
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

    void MouseInput()
    {
        if (!Input.GetMouseButtonUp(0) || !canPutCell || _cameraMove.isMoving) return;
        
        // 鼠标不在UI上
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        // Debug.Log("MouseDown");
            
        // 根据鼠标位置计算棋盘位置和矩阵位置
        Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 localMousePosition = transform.InverseTransformPoint(mousePosition) / (cellSize + cellInterval);
        int posX = Convert.ToInt32(localMousePosition.x);
        int posY = Convert.ToInt32(localMousePosition.y);
        // Debug.Log(posX);
        // Debug.Log(posY);

        // 调整矩阵状态
        ExtendTo(posX, posY);
        Cell currentCell = Board[- posY + Size/2, posX + Size/2];
        if (currentCell.Status)
        {
            currentCell.Hide();
        }
        else
        {
            currentCell.Display();
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
        pool = GetComponent<BoardCellPool>();
        pool.cellObject = cellObject;
        pool.sizeLimit = sizeLimit;
    }

    void Start()
    {
        cellObject.transform.localScale = new Vector3(cellSize, cellSize, 1);
        _nextUpdateTime = updateInterval;
        _cameraMove = _camera.GetComponent<CameraMove>();
        Board = new Cell[Size, Size];
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Board[i, j] = new Cell();
                Board[i, j].SetPos(i, j);
            }
        }
    }

    void Update()
    {
        // System.Diagnostics.Stopwatch stopwatch = new();
        // stopwatch.Start();
        if (_canUpdate && !_isContinuousUpdate)
        {
            UpdateBoard();
            _canUpdate = false;
        }

        if (_canUpdate && _isContinuousUpdate)
        {
            if (_nextUpdateTime < 0)
            {
                UpdateBoard();
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
}
