using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChessboardController : MonoBehaviour
{
    static ChessboardController _instance;
    public static ChessboardController Instance {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            
            if (_instance == null)
            {
                _instance = FindObjectOfType<ChessboardController>();
            }

            if (_instance == null)
            {
                GameObject obj = new GameObject("Chessboard");
                _instance = obj.AddComponent<ChessboardController>();
            }

            return _instance;
        }
    }


    [Header("Game Rules")]
    [SerializeField][Range(0, 8)] int surviveDownThreshold = 2;
    [SerializeField][Range(0, 8)] int surviveUpThreshold = 3;
    [SerializeField][Range(0, 8)] int bornDownThreshold = 3;
    [SerializeField][Range(0, 8)] int bornUpThreshold = 3;
    
    [Header("Board Settings")]
    [SerializeField] float cellInterval = 0f;
    [SerializeField] float cellSize = 0.5f;
    [SerializeField] float updateInterval = 1f;
    [SerializeField] GameObject boardCell;
    [SerializeField] GameObject boardHolder; // 棋盘格子放在这个GameObject下
    [SerializeField] GameObject updateStatusText;

    bool _canUpdate;
    public bool canPutCell = true;
    bool _isContinuousUpdate;
    float _nextUpdateTime;
    Camera _camera;
    CameraMove _cameraMove;
    Text _updateStatusText;

    BoardCellPool _pool;

    // 列向量
    public Chessboard Board;

    void BoardUpdate()
    {
        _pool.ClearCell();
        Board.UpdateBoard();
        
        // 根据状态更新棋盘
        for (int i = 0; i < Board.Columns; i++)
        {
            for (int j = 0; j < Board.Rows; j++)
            {
                if (!Board.Status[i, j]) continue;
                Vector3 boardPosition = new Vector3(i - Board.Columns / 2, j - Board.Rows / 2);
                GameObject cell = _pool.GetCell();
                cell.transform.localPosition = boardPosition * (cellSize + cellInterval);
                cell.name = $"BoardCell{boardPosition.x}, {boardPosition.y}";
                Board.ExtendTo(i - Board.Columns/2, j - Board.Rows/2);
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
        StopUpdate();
        Board.Clear();
        _pool.ClearCell();
    }

    public int[] GetBoardSize()
    {
        return new[]{Board.Rows, Board.Columns};
    }
    
    void MouseInput()
    {
        if (!Input.GetMouseButtonUp(0) || !canPutCell || _cameraMove.isMoving) return;
        
        // 鼠标不在UI上
        if (EventSystem.current.IsPointerOverGameObject()) return;
            
        // 根据鼠标位置计算棋盘位置和矩阵位置
        Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 localMousePosition =
            boardHolder.transform.InverseTransformPoint(mousePosition) / (cellSize + cellInterval);
        int boardX = Convert.ToInt32(localMousePosition.x);
        int boardY = Convert.ToInt32(localMousePosition.y);

        // 调整矩阵状态
        Board.ExtendTo(boardX, boardY);
        Board.Status[boardX + Board.Columns/2, boardY + Board.Rows/2] = true;
            
        // 实例化棋盘单元
        GameObject cell = _pool.GetCell();
        cell.transform.localPosition = new Vector3(boardX, boardY, 0) * (cellSize + cellInterval);
        cell.name = $"BoardCell{boardX}, {boardY}";
    }

    void Awake()
    {
        if (Instance != this)
        {
            DestroyImmediate(this);
            return;
        }

        Board = new Chessboard(21, 21, surviveDownThreshold, surviveUpThreshold, bornDownThreshold, bornUpThreshold);
        _camera = Camera.main;
        _updateStatusText = updateStatusText.GetComponent<Text>();
    }

    void Start()
    {
        boardCell.transform.localScale = new Vector3(cellSize, cellSize, 1);
        _nextUpdateTime = updateInterval;

        _cameraMove = _camera.GetComponent<CameraMove>();

        // init chessboard
        if (boardHolder == null)
            boardHolder = GameObject.Find("BoardPool");
        if (boardHolder == null)
        {
            boardHolder = new GameObject("BoardPool");
            boardHolder.AddComponent<BoardCellPool>();
        }

        _pool = boardHolder.GetComponent<BoardCellPool>();
        _pool.boardCell = boardCell;
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
        
        MouseInput();
        
    }
}
