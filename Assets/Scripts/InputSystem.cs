using System;
using QFramework;
using UnityEngine;


public class InputSystem : AbstractSystem
{
    PlayerInput _input;
    
    public PlayerInput.PlayerActions PlayerActionMap { get; private set; }
    public PlayerInput.CameraActions CameraActions { get; private set; }

    protected override void OnInit()
    {
        _input = new PlayerInput();
        PlayerActionMap = _input.Player;
        CameraActions = _input.Camera;
    }
}