using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CameraMove : MonoBehaviour, IController
{
    [SerializeField] float _moveSpeed = 0.1f;
    [SerializeField] float _zoomScale = 1.1f;
    [SerializeField] float _zoomSpeed = 0.1f;
    [SerializeField] float _zoomUp = 50f;
    [SerializeField] float _zoomDown = 5f;
    [SerializeField] Transform _background;
    
    Camera _camera;
    float _targetSize;

    bool _isMoving;
    bool _isZooming;
    
    public bool IsMoving => _isMoving && _isZooming;

    [Button]
    void ResetView()
    {
        _camera.orthographicSize = 5f;
        transform.position = new Vector3(0, 0, -10);
        _background.position = Vector3.zero;
        _background.localScale = Vector3.one;
    }
    

    async void MoveAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var startMousePosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var startPosition = transform.position;
            _isMoving = true;
            
            
            while (_isMoving)
            {
                var currentMousePosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var targetPosition = startPosition - (currentMousePosition - startMousePosition);

                if (Vector3.Magnitude(targetPosition - transform.position) > 0.01f && Time.deltaTime < _moveSpeed)
                {
                    transform.position += (targetPosition - transform.position) * Time.deltaTime / _moveSpeed;
                }
                else
                {
                    transform.position = targetPosition;
                }

                if (Vector2.Distance(_background.position, transform.position) > 10)
                {
                    var direction = transform.position - _background.position;
                    _background.position += new Vector3((int)direction.x, (int)direction.y, 0);
                }

                await UniTask.WaitForEndOfFrame();
            }
        }

        if (context.canceled)
        {
            _isMoving = false;
        }
    }

    async void ZoomAction(InputAction.CallbackContext context)
    {
        var scroll = context.ReadValue<Vector2>().y;
        switch (scroll)
        {
            case < 0:
                _targetSize *= _zoomScale;
                break;
            case > 0:
                _targetSize /= _zoomScale;
                break;
        }
        
        if (_targetSize > _zoomUp)
        {
            _targetSize = _zoomUp;
        }

        if (_targetSize < _zoomDown)
        {
            _targetSize = _zoomDown;
        }
        
        var targetScale = (int)_targetSize / 5;
        if (targetScale % 2f != 0)
        {
            _background.localScale = new Vector3(targetScale, targetScale, 1);
        }

        if (_isZooming) return;
        _isZooming = true;
        while (_isZooming)
        {
            
            var size = _camera.orthographicSize;
            // 背景格子缩放
            if (Mathf.Abs(size - _targetSize) > 0.01f && Time.deltaTime < _zoomSpeed)
            {
                _camera.orthographicSize += (_targetSize - size) * Time.deltaTime / _zoomSpeed;
            }
            else
            {
                _camera.orthographicSize = _targetSize;
                _isZooming = false;
            }
            
            await UniTask.WaitForEndOfFrame();
        }
    }

    void RegisterInput()
    {
        var cameraInput = this.GetSystem<InputSystem>().CameraActions;
        cameraInput.Move.performed += MoveAction;
        cameraInput.Move.canceled += MoveAction;
        cameraInput.Zoom.performed += ZoomAction;
    }

    void UnregisterInput()
    {
        var cameraInput = this.GetSystem<InputSystem>().CameraActions;
        cameraInput.Move.performed -= MoveAction;
        cameraInput.Move.canceled -= MoveAction;
        cameraInput.Zoom.performed -= ZoomAction;
        
    }
    
    void Start()
    {
        _camera = Camera.main;
        if (_camera != null) _targetSize = _camera.orthographicSize;
    }

    void OnEnable()
    {
        this.GetSystem<InputSystem>().CameraActions.Enable();
        RegisterInput();
    }

    void Update()
    {
    }

    void OnDisable()
    {
        this.GetSystem<InputSystem>().CameraActions.Disable();
        UnregisterInput();
    }

    public IArchitecture GetArchitecture()
    {
        return GameOfLife.Interface;
    }
}
