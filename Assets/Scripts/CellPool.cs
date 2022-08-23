using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class CellPool : MonoBehaviour
{
    static CellPool _instance;
    public static CellPool Instance {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            
            if (_instance == null)
            {
                _instance = FindObjectOfType<CellPool>();
            }

            if (_instance == null)
            {
                var obj = new GameObject("CellPool");
                _instance = obj.AddComponent<CellPool>();
            }

            return _instance;
        }
    }

    [SerializeField] int poolSizeStep = 500;
    
    public float cellInterval = 0f;
    public float cellSize = 0.5f;
    public GameObject cellObject;

    Transform _transform;
    
    readonly List<Cell> _cellPool = new();
    

    public class Cell
    {
        readonly GameObject _cellObject;
        readonly BoardCell _boardCell;
        public bool Status;
        public bool NextStatus;

        public Cell(GameObject cellObject, Transform transform)
        {
            _cellObject = Instantiate(cellObject, transform);
            _boardCell = _cellObject.GetComponent<BoardCell>();
            
            _boardCell.Hide();
        }

        // public bool CanChange => _boardCell.CanChange;
        
        public void InitialisePos(Transform transform)
        {
            _cellObject.transform.parent = transform;
        }

        public void SetPos(int i, int j, Transform transform)
        {
            Vector3 pos = new(j - Chessboard.Instance.size / 2, Chessboard.Instance.size / 2 - i);
            _cellObject.name = $"BoardCell{pos.x}, {pos.y}";
            _cellObject.transform.parent = transform;
            _cellObject.transform.localPosition = pos * (Instance.cellSize + Instance.cellInterval);
        }

        public void Enable()
        {
            Status = true;
            _boardCell.Enable();
        }

        public void Disable()
        {
            Status = false;
            _boardCell.Disable();
        }

        public void Hide()
        {
            Status = false;
            _boardCell.Hide();
        }

        public void UpdateStatus()
        {
            if(NextStatus) Enable();
            else Disable();
        }

        public void ChangeStatus()
        {
            if(Status) Disable();
            else Enable();
            
        }
    }

    void IncreasePoolSize(int n)
    {
        for (var i = 0; i < n; i++)
        {
            var cell = new Cell(cellObject, _transform);
            _cellPool.Add(cell);
        }
    }
    
    public Cell GetCell()
    {
        if(_cellPool.Count > 0)
        {
            var cell = _cellPool[0];
            cell.Disable();
            _cellPool.RemoveAt(0);
            return cell;
        }
        else
        {
            // Debug.Log("pool insufficient");
            IncreasePoolSize(poolSizeStep);
            var cell = _cellPool[0];
            cell.Disable();
            _cellPool.RemoveAt(0);
            return cell;
        }
    }

    public void AddCell(Cell cell)
    {
        _cellPool.Add(cell);
        cell.InitialisePos(_transform);
        cell.Hide();
    }

    void Awake()
    {
        _transform = transform;
        cellObject.transform.localScale = new Vector3(cellSize, cellSize, 1);
        IncreasePoolSize(poolSizeStep);
    }

    void LateUpdate()
    {
        var size = Chessboard.Instance.size;
        var sizeLimit = Chessboard.Instance.sizeLimit;
        if (size >= sizeLimit) return;
        if (_cellPool.Count < 3 * size * size + 4 * size + 1)
        {
            IncreasePoolSize(3 * size  + 4);
        }
    }
}
