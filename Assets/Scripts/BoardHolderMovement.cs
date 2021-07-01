using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BoardHolderMovement : MonoBehaviour
{
    [SerializeField] float smoothAmount = 15f;
    Vector3 _startMousePosition;
    Vector3 _startBoardPosition;
    Vector3 _targetPosition;
    Camera _camera;
    Transform _transform;

    [HideInInspector] public bool isBoardMoving;

    void Start()
    {
        _camera = Camera.main;
        _transform = transform;
    }

    void Update()
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
            _transform.position = Vector3.Lerp(_transform.position, _targetPosition, smoothAmount*Time.deltaTime);
        }
        else
        {
            isBoardMoving = false;
        }
    }
}
