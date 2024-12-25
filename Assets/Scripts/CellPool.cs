using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

public class CellPool : MonoBehaviour
{
    [SerializeField] int _poolSizeStep = 500;
    [SerializeField] AssetReference _cellObject;
    
    AsyncOperationHandle<GameObject> _cellObjectHandle;

    readonly Stack<Cell> _cellPool = new();
    

    public class Cell
    {
        BoardCell _boardCell;
        public bool Status;

        public Cell(BoardCell boardCell, Transform transform)
        {
            _boardCell = boardCell;
            _boardCell.Disable();
        }

        public void SetPos(Vector2Int pos)
        {
            var cellObject = _boardCell.gameObject;
            cellObject.name = $"BoardCell{pos.x}, {pos.y}";
            cellObject.transform.localPosition = new Vector2(pos.x, pos.y);
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

        public void Destroy()
        {
            if (_boardCell != null)
            {
                _boardCell.Destroy();
                _boardCell = null;
            }
        }
    }

    async UniTask<Cell> CreateCell()
    {
        await _cellObjectHandle;
        if (_cellObjectHandle.Result == null) return null;
        var obj = Instantiate(_cellObjectHandle.Result, transform);
        var cell = new Cell(obj.GetOrAddComponent<BoardCell>(), transform);
        return cell;
    }

    async UniTask IncreasePoolSize(int n)
    {
        for (var i = 0; i < n; i++)
        {
            var cell = await CreateCell();
            _cellPool.Push(cell);
        }
    }
    
    public async UniTask<Cell> Pop()
    {
        if(_cellPool.Count <= 0)
        {
            // Debug.Log("pool insufficient");
            await IncreasePoolSize(_poolSizeStep);
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

    async void Awake()
    {
        _cellObjectHandle = Addressables.LoadAssetAsync<GameObject>(_cellObject);
        await IncreasePoolSize(_poolSizeStep);
    }

    async void Start()
    { 
        await _cellObjectHandle;
    }

    public void Clear()
    {
        foreach (var cell in _cellPool)
        {
            cell.Destroy();
        }
        
        _cellPool.Clear();
    }


    void OnDestroy()
    {
        if (_cellObjectHandle.IsValid())
        {
           Addressables.Release(_cellObjectHandle); 
        }
        
        Clear();
    }

    void LateUpdate()
    {
    }
}
