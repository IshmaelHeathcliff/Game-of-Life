using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputComponent : MonoBehaviour
{
    public class InputButton
    {
        public KeyCode Key { get; set; }

        public bool Down { get; set; }
        public bool Up { get; set; }
        public bool Held{ get; set; }
        public bool Enabled { get; private set; } = true;
        
        bool _afterFixedUpdateDown;
        bool _afterFixedUpdateUp;
        bool _afterFixedUpdateHeld;

        bool _canGetInput = true;

        public InputButton(KeyCode key)
        {
            Key = key;
        }

        public void Enable()
        {
            Enabled = true;
        }
        
        public void Disable()
        {
            Enabled = false;
        }

        public void GainControl()
        {
            _canGetInput = true;
        }

        public IEnumerator ReleaseControl(bool resetValue)
        {
            _canGetInput = false;
            
            if (!resetValue)
                yield break;

            if (Down)
                Up = true;
            Down = false;
            Held = false;

            _afterFixedUpdateDown = false;
            _afterFixedUpdateUp = false;
            _afterFixedUpdateHeld = false;

            yield return null;

            Up = false;
        }

        public void Get(bool fixedUpdateHappened)
        {
            if (!Enabled)
            {
                Down = false;
                Up = false;
                Held = false;
                return;
            }

            if (!_canGetInput) 
                return;

            if (fixedUpdateHappened)
            {
                Down = Input.GetKeyDown(Key);
                Up = Input.GetKeyUp(Key);
                Held = Input.GetKey(Key);

                _afterFixedUpdateDown = Down;
                _afterFixedUpdateUp = Up;
                _afterFixedUpdateHeld = Held;
            }
            else
            {
                Down = Input.GetKeyDown(Key) || _afterFixedUpdateDown;
                Up = Input.GetKeyUp(Key) || _afterFixedUpdateUp;
                Held = Input.GetKey(Key) || _afterFixedUpdateHeld;

                _afterFixedUpdateDown |= Down;
                _afterFixedUpdateUp |= Up;
                _afterFixedUpdateHeld |= Held;
            }
            
        }
    }
    
    bool _fixedUpdateHappened;

    void FixedUpdate()
    {
        _fixedUpdateHappened = true;
    }

    void Update()
    {
        GetInputs(_fixedUpdateHappened);

        _fixedUpdateHappened = false;
    }

    protected abstract void GetInputs(bool fixedUpdateHappened);

    public abstract void GainControl();
    
    public abstract void ReleaseControl(bool resetValue=true);

    protected void GainControl(InputButton inputButton)
    {
        inputButton.GainControl();
    }

    protected void ReleaseControl(InputButton inputButton, bool resetValue)
    {
        StartCoroutine(inputButton.ReleaseControl(resetValue));
    }

}
