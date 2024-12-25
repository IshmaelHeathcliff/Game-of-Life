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
                _instance = FindAnyObjectByType<CellPool>();
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
    public float cellSize = 1f;
    public GameObject cellObject;

    Transform _transform;
    
    readonly Stack<Cell> _cellPool = new();
    

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
            
            _boardCell.Disable();
        }

        // public bool CanChange => _boardCell.CanChange;
        
        public void InitialisePos(Transform transform)
        {
            _cellObject.transform.parent = transform;
        }

        public void SetPos(Vector2Int pos, Transform transform)
        {
            _cellObject.name = $"BoardCell{pos.x}, {pos.y}";
            _cellObject.transform.parent = transform;
            _cellObject.transform.localPosition = new Vector2(pos.x, pos.y) * (Instance.cellSize + Instance.cellInterval);
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
            _cellPool.Push(cell);
        }
    }
    
    public Cell Pop()
    {
        if(_cellPool.Count <= 0)
        {
            // Debug.Log("pool insufficient");
            IncreasePoolSize(poolSizeStep);
        }

        var cell = _cellPool.Pop();
        cell.Enable();
        return cell;
    }

    public void Push(Cell cell)
    {
        _cellPool.Push(cell);
        cell.Disable();
    }

    void Awake()
    {
        _transform = transform;
        cellObject.transform.localScale = new Vector3(cellSize, cellSize, 1);
        IncreasePoolSize(poolSizeStep);
    }

    void LateUpdate()
    {
    }
}
