using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class BoardCell: MonoBehaviour
{
    bool _status;
    public bool Status {
        get => _status;
        set
        {
            _status = value;
            _renderer.sprite = Chessboard.Instance.cellBundle.LoadAsset(value ? "white.png" : "black.png") as Sprite;
        }
    }
    public bool NextStatus { get; set; }

    SpriteRenderer _renderer;
    bool _checkMouse;

    public void ChangeStatus()
    {
        Status = !Status;
    }

    public void Reset()
    {
        Status = false;
    }

    void PointerEnter()
    {
        _checkMouse = true;
    }

    void PointerExit()
    {
        _checkMouse = false;
    }
    

    void AddPointEvent()
    {
        var eventTrigger = GetComponent<EventTrigger>();

        eventTrigger.triggers = new List<EventTrigger.Entry>();

        var pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter,
            callback = new EventTrigger.TriggerEvent()
        };
        
        var pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit,
            callback = new EventTrigger.TriggerEvent()
        };

        pointerEnter.callback.AddListener((data) => { PointerEnter(); });
        pointerExit.callback.AddListener((data) => { PointerExit(); });

        eventTrigger.triggers.Add(pointerEnter);
        eventTrigger.triggers.Add(pointerExit);
    }

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        AddPointEvent();
    }

    void Update()
    {
        if (_checkMouse && Input.GetMouseButtonUp(0))
        {
            ChangeStatus();
        }
            
    }

}