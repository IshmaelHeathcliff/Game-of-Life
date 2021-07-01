using UnityEngine;

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

    [SerializeField] int height = 10;
    [SerializeField] int width = 10;
    [SerializeField][Range(0, 8)] int liveUpThreshold = 3;
    [SerializeField][Range(0, 8)] int liveDownThreshold = 2;
    [SerializeField][Range(0, 8)] int bornUpThreshold = 3;
    [SerializeField][Range(0, 8)] int bornDownThreshold = 3;
    [SerializeField] float cellInterval = 0.1f;
    [SerializeField] float cellSize = 0.5f;
    [SerializeField] GameObject boardCell;
    [SerializeField] GameObject boardHolder; // 棋盘格子放在这个GameObject下
    [SerializeField] float updateInterval = 1f;

    BoardCell[,] _chessboard;
    Vector3 _cellOffset;
    bool _canUpdate;
    bool _isContinuousUpdate;
    float _nextUpdateTime;

    // [HideInInspector] public AssetBundle cellBundle;
    public Sprite whiteCell;
    public Sprite blackCell;


    public void UpdateOnce()
    {
        _canUpdate = true;
        _isContinuousUpdate = false;
    }

    public void StartUpdate()
    {
        _isContinuousUpdate = true;
        _canUpdate = true;
    }

    public void StopUpdate()
    {
        _canUpdate = false;
        _isContinuousUpdate = false;
    }

    public void ClearBoard()
    {
        StopUpdate();
        foreach (BoardCell cell in _chessboard)
        {
            cell.Reset();
        }
    }

    void Awake()
    {
        if (Instance != this)
        {
            DestroyImmediate(this);
            return;
        }
        
        _chessboard = new BoardCell[width, height];
    }
    
    void Start()
    {
        // cellBundle = AssetBundle.LoadFromFile("Assets/Artworks/Bundles/cell.unity3d");
        boardCell.transform.localScale = new Vector3(cellSize, cellSize, 1);
        _cellOffset = new Vector3(-(width-1f)/2f, -(height-1f)/2f, 0f);
        _nextUpdateTime = updateInterval;
        
        // init chessboard
        if (boardHolder == null)
            boardHolder = GameObject.Find("Board");
        if (boardHolder == null)
        {
            boardHolder = new GameObject("Board");
            boardHolder.AddComponent<BoardHolderMovement>();
        }
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var cell = Instantiate(boardCell, boardHolder.transform);
                cell.transform.localPosition = (new Vector3(i, j, 0) + _cellOffset) * (cellSize + cellInterval);
                cell.name = $"BoardCell{i}, {j}";
                _chessboard[i, j] = cell.GetComponent<BoardCell>();
            }
        }
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

    void BoardUpdate()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                bool status = _chessboard[i, j].Status;
                int liveNeighbours = 0;
                for (int r = -1; r < 2; r++)
                {
                    for (int c = -1; c < 2; c++)
                    {
                        if (r == 0 && c == 0) continue;
                        int rr = r + i;
                        int cc = c + j;
                        if ((rr > 0 && rr < width) && (cc > 0 && cc < height) && _chessboard[rr, cc].Status)
                            liveNeighbours++;
                    }
                }
                
                // if(Board[i,j].Status) Debug.Log($"{i},{j}:{liveNeighbours}");

                if (liveNeighbours < liveDownThreshold || liveNeighbours > liveUpThreshold)
                {
                    _chessboard[i, j].NextStatus = false;
                }
                else
                {
                    if (status) _chessboard[i, j].NextStatus = true;
                    else if(liveNeighbours >= bornDownThreshold && liveNeighbours <= bornUpThreshold) _chessboard[i, j].NextStatus = true;
                    else _chessboard[i, j].NextStatus = false;
                }
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(_chessboard[i,j].Status != _chessboard[i,j].NextStatus) _chessboard[i,j].ChangeStatus();
                _chessboard[i, j].Status = _chessboard[i, j].NextStatus;
            }
        }
    }
}
