using EX;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputProcessor
{
    public static PlayerInput input;
    public static bool inputDirectionPressedThisFrame;
    public static Vector2 inputDirection;
    public static Vector2 inputDirection4Way;
    public static Vector2 inputDirectionLast4Way;

    public static void GetInputs()
    {
        inputDirection = input.actions["MoveDirection"].ReadValue<Vector2>();
        inputDirectionPressedThisFrame = input.actions["MoveDirection"].WasPressedThisFrame();

        inputDirection4Way = inputDirection;
        if (inputDirection.y != 0 && inputDirection.x != 0)
            inputDirection4Way = inputDirection4Way.SetX(0);

        if (inputDirection4Way != Vector2.zero)
            inputDirectionLast4Way = inputDirection4Way;
    }
}
