using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BoardHolderMovement : MonoBehaviour
{
    [SerializeField] float moveSmoothAmount = 15f;
    [SerializeField] float zoomSpeed = 1.1f;
    [SerializeField] float zoomSmoothAmount = 5f;
    Vector3 _startMousePosition;
    Vector3 _startBoardPosition;
    Vector3 _targetPosition;
    Camera _camera;
    Transform _transform;
    float _targetSize;

    [HideInInspector] public bool isBoardMoving;
    

    void Start()
    {
        _camera = Camera.main;
        _transform = transform;
        _targetSize = _transform.localScale.x;
    }
    
    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0) _targetSize *= zoomSpeed;
        else if (scroll < 0) _targetSize /= zoomSpeed;

        var targetScale = new Vector2(_targetSize, _targetSize);

        if (Mathf.Abs(_transform.localScale.x - _targetSize) > 0.01f)
        {
            _transform.localScale = Vector2.Lerp(_transform.localScale, targetScale, zoomSmoothAmount*Time.deltaTime);
            isBoardMoving = true;
        }
        else
        {
            isBoardMoving = false;
        }
        
    }

    void Move()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _startMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _startBoardPosition = _transform.position;
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 currentMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _targetPosition = currentMousePosition - _startMousePosition + _startBoardPosition;
        }

        if (Vector3.Magnitude(_targetPosition - _transform.position) > 0.1f)
        {
            isBoardMoving = true;
            _transform.position = Vector3.Lerp(_transform.position, _targetPosition, moveSmoothAmount*Time.deltaTime);
        }
        else
        {
            isBoardMoving = false;
        }
    }

    void Update()
    {
        Move();
        Zoom();
    }
}
