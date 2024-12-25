using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraMove : MonoBehaviour
{
    [SerializeField] float moveTime = 0.1f;
    [SerializeField] float zoomSpeed = 1.1f;
    [SerializeField] float zoomTime = 0.2f;
    [SerializeField] float zoomUp = 200f;
    [SerializeField] float zoomDown = 5f;
    [SerializeField] Transform background;
    
    Vector3 _startMousePosition;
    Vector3 _startBoardPosition;
    Vector3 _targetPosition;
    Camera _camera;
    float _targetSize;
    
    
    [HideInInspector] public bool isMoving;
    void Start()
    {
        _camera = Camera.main;
        if (_camera != null) _targetSize = _camera.orthographicSize;
    }
    
    void Zoom()
    {
        var size = _camera.orthographicSize;
        var scale = transform.localScale;
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        switch (scroll)
        {
            case < 0:
                _targetSize *= zoomSpeed;
                break;
            case > 0:
                _targetSize /= zoomSpeed;
                break;
        }

        if (_targetSize > zoomUp)
        {
            _targetSize = zoomUp;
        }

        if (_targetSize < zoomDown)
        {
            _targetSize = zoomDown;
        }

        var targetScale = (int)_targetSize / 5;
        background.localScale = new Vector3(targetScale, targetScale, 1);

        if (Mathf.Abs(size - _targetSize) > 0.01f && Time.deltaTime < moveTime)
        {
            _camera.orthographicSize += (_targetSize - size) * Time.deltaTime / zoomTime;
            isMoving = true;
        }
        else
        {
            _camera.orthographicSize = _targetSize;
            isMoving = false;
        }
       
    }

    void Move()
    {
        var position = transform.position;
        _targetPosition = position;
        
        if (Input.GetMouseButtonDown(1))
        {
            _startMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _startBoardPosition = transform.position;
        }
        if (Input.GetMouseButton(1))
        {
            var currentMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _targetPosition = _startBoardPosition - (currentMousePosition - _startMousePosition);
        }


        if (Vector3.Magnitude(_targetPosition - position) > 0.01f && Time.deltaTime < moveTime)
        {
            isMoving = true;
            transform.position += (_targetPosition - position) * Time.deltaTime / moveTime;
        }
        else
        {
            transform.position = _targetPosition;
            isMoving = false;
        }
    }

    void Update()
    {
        Move();
        Zoom();
    }
}
