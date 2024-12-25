using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = System.Random;

[RequireComponent(typeof(CellPool))]
public class Chessboard : MonoBehaviour, IController
{
    [Header("Game Rules")] 
    [SerializeField] [Range(0, 8)] int _surviveDownThreshold = 2;

    [SerializeField] [Range(0, 8)] int _surviveUpThreshold = 3;
    [SerializeField] [Range(0, 8)] int _bornDownThreshold = 3;
    [SerializeField] [Range(0, 8)] int _bornUpThreshold = 3;
    [SerializeField] int _randomDensity = 3;

    [Header("Board Settings")] 
    [SerializeField][Range(0.1f, 1)] float _updateInterval = 1f;
    [SerializeField] TextMeshProUGUI _updateStatusText;
    
    bool _canPutCell = true;
    bool _isUpdating;
    Camera _camera;
    CameraMove _cameraMove;
    CellPool _pool;

    readonly Dictionary<Vector2Int, CellPool.Cell> _grid = new();

    int CountLiveNeighbors(Vector2Int pos)
    {
        var count = 0;
        for (var i = -1; i <= 1; i++)
        {
            for (var j = -1; j <= 1; j++)
            {
                var neighborPos = new Vector2Int(pos.x + i, pos.y + j);
                if (neighborPos != pos && _grid.ContainsKey(neighborPos) &&
                    _grid[neighborPos].Status)
                {
                    count++;
                }
            }
        }

        return count;
    }

    async UniTask UpdateGrid()
    {
        var newGrid = new Dictionary<Vector2Int, bool>();
        var cellsToCheck = new HashSet<Vector2Int>(_grid.Keys);
        foreach (var pos in _grid.Keys)
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
            var isAlive = _grid.ContainsKey(pos) && _grid[pos].Status;
            if (isAlive && (liveNeighbors < _surviveDownThreshold || liveNeighbors > _surviveUpThreshold))
            {
                newGrid[pos] = false;
            }
            else if (!isAlive && (liveNeighbors >= _bornDownThreshold && liveNeighbors <= _bornUpThreshold))
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
                if (!_grid.ContainsKey(item.Key))
                {
                    _grid[item.Key] = await _pool.Pop();
                }

                _grid[item.Key].SetPos(item.Key);
            }
            else
            {
                if (_grid.ContainsKey(item.Key))
                {
                    _pool.Push(_grid[item.Key]);
                    _grid.Remove(item.Key);
                }
            }
        }
    }

    async void MouseInput()
    {
        if (!_canPutCell || _cameraMove.IsMoving) return;

        // 确保鼠标不在UI上，替代下一行
        // if (EventSystem.current.IsPointerOverGameObject()) return;
        var pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> result = new();
        EventSystem.current.RaycastAll(pointerEventData, result);
        if (result.Count > 0) return;
        

        // Debug.Log("MouseDown");

        // 根据鼠标位置计算棋盘位置和矩阵位置
        var mousePosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        var localMousePosition = transform.InverseTransformPoint(mousePosition);
        var posX = Convert.ToInt32(localMousePosition.x);
        var posY = Convert.ToInt32(localMousePosition.y);
        var pos = new Vector2Int(posX, posY);
        // Debug.Log(posX);
        // Debug.Log(posY);

        if (!_grid.ContainsKey(pos))
        {
            _grid[pos] = await _pool.Pop();
            _grid[pos].SetPos(pos);
        }
        else
        {
            _pool.Push(_grid[pos]);
            _grid.Remove(pos);
        }
    }

    [Button]
    public async void UpdateOnce()
    {
        System.Diagnostics.Stopwatch stopwatch = new();
        stopwatch.Start();
        
        await UpdateGrid();
        _updateStatusText.text = "Finished";
        
        Debug.Log(transform.childCount);
        stopwatch.Stop();
        double updateTime = stopwatch.Elapsed.TotalMilliseconds;
        if(updateTime > 1)
            Debug.Log($"update time: {updateTime}");
    }

    [Button]
    public async void StartUpdate()
    {
        if (_isUpdating) return;
        
        _isUpdating = true;
        _updateStatusText.text = "On";
        _canPutCell = false;

        while (_isUpdating)
        {
            await UpdateGrid();
            await UniTask.Delay((int)(_updateInterval * 1000));
        }
    }

    [Button]
    public void StopUpdate()
    {
        _isUpdating = false;
        _updateStatusText.text = "Off";
        _canPutCell = true;
    }

    [Button]
    public async UniTask Randomize(Vector2Int size)
    {
        var rd = new Random();
        for (var i = -size.x; i < size.x; i++)
        {
            for (var j = -size.y; j < size.y; j++)
            {
                var pos = new Vector2Int(i, j);
                if (rd.Next(_randomDensity) < 1)
                {
                    if (_grid.ContainsKey(pos)) continue;
                    
                    _grid[pos] = await _pool.Pop();
                    _grid[pos].SetPos(pos);
                }
                else
                {
                    if (!_grid.TryGetValue(pos, out var cell)) continue;
                    
                    _pool.Push(cell);
                    _grid.Remove(pos);
                }
            }
            
        }
    }
    
    [Button]
    public void Clear()
    {
        StopUpdate();
        foreach (var item in _grid)
        {
            item.Value.Destroy();
        }
        _grid.Clear();
        _pool.Clear();
        GC.Collect();
    }

    void UpdateAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            UpdateOnce();
        }

        if (context.performed)
        {
            StartUpdate();
        }

        if (context.canceled && _isUpdating)
        {
            StopUpdate();
        }
    }

    void CellAction(InputAction.CallbackContext context)
    {
        MouseInput();
    }
    
    void RegisterInput()
    {
        var playerInput = this.GetSystem<InputSystem>().PlayerActionMap;
        playerInput.Update.started += UpdateAction;
        playerInput.Update.performed += UpdateAction;
        playerInput.Update.canceled += UpdateAction;
        playerInput.Cell.performed += CellAction;
    }

    void UnregisterInput()
    {
        var playerInput = this.GetSystem<InputSystem>().PlayerActionMap;
        playerInput.Update.performed -= UpdateAction;
        playerInput.Update.performed -= UpdateAction;
        playerInput.Update.canceled -= UpdateAction;
        playerInput.Cell.performed -= CellAction;
    }
    
    void Awake()
    {
        // Application.targetFrameRate = 60;
        _camera = Camera.main;
        _pool = GetComponent<CellPool>();
    }

    void OnEnable()
    {
        this.GetSystem<InputSystem>().PlayerActionMap.Enable();
        RegisterInput();
    }

    void Start()
    {
        _cameraMove = _camera.GetComponent<CameraMove>();
    }

    void Update()
    {
    }

    void OnDisable()
    {
        this.GetSystem<InputSystem>().PlayerActionMap.Disable();
        UnregisterInput();
    }

    void OnDestroy()
    {
        Clear();
        GC.Collect();
    }

    public IArchitecture GetArchitecture()
    {
        return GameOfLife.Interface;
    }
}