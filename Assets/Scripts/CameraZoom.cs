using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    Camera _camera;

    [SerializeField] float zoomSpeed = 1.1f;
    [SerializeField] float smoothAmount = 5f;

    float _targetCameraSize;
    
    void Start()
    {
        _camera = GetComponent<Camera>();
        _targetCameraSize = _camera.orthographicSize;
    }

    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0) _targetCameraSize *= zoomSpeed;
        else if (scroll > 0) _targetCameraSize /= zoomSpeed;
        
        if(Mathf.Abs(_camera.orthographicSize - _targetCameraSize) > Mathf.Epsilon) 
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetCameraSize, smoothAmount*Time.deltaTime);
    }


    void Update()
    {
        Zoom();
    }
}
