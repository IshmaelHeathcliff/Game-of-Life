using QFramework;
using UnityEngine;

public class GameOfLife : Architecture<GameOfLife>
{
    protected override void Init()
    {
        RegisterSystem(new InputSystem());
    }
}
