using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerInput : InputComponent
{
    static PlayerInput _instance;
    public static PlayerInput Instance => _instance;

    #region Buttons
    // 按键设置, 需在Awake()中添加至Buttons
    public readonly InputButton UpdateOnce = new InputButton(KeyCode.Space);


    #endregion

    
    
    readonly List<InputButton> Buttons = new List<InputButton>();

    public bool HaveControl { get; private set; } = true;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
        Buttons.Add(UpdateOnce);
    }

    protected override void GetInputs(bool fixedUpdateHappened)
    {
        foreach (InputButton button in Buttons) 
            button.Get(fixedUpdateHappened);
    }

    public override void GainControl()
    {
        HaveControl = true;

        foreach (InputButton button in Buttons) 
            GainControl(button);
    }

    public override void ReleaseControl(bool resetValue = true)
    {
        HaveControl = false;

        foreach (InputButton button in Buttons)
            ReleaseControl(button, resetValue);
    }
}
