using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class BoardCell : MonoBehaviour
{
    SpriteRenderer _renderer;

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        var c = _renderer.color;
        _renderer.color = new Color(c.r, c.g, c.b, 0);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }
}
