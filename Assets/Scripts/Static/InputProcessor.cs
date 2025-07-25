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
    public static Vector2 inputDirectionBuffered4Way;
    const float INPUT_BUFFER_DURATION = .1f;
    static float timeAtReleasedInput;

    public static void GetInputs()
    {
        inputDirection = input.actions["MoveDirection"].ReadValue<Vector2>();
        inputDirectionPressedThisFrame = input.actions["MoveDirection"].WasPressedThisFrame();

        inputDirection4Way = inputDirection;
        if (inputDirection.y != 0 && inputDirection.x != 0)
            inputDirection4Way = inputDirection4Way.SetX(0);

        inputDirection4Way = inputDirection4Way.normalized;

        if (inputDirection4Way != Vector2.zero)
        {
            inputDirectionLast4Way = inputDirection4Way;
            inputDirectionBuffered4Way = inputDirection4Way;
        }

        if (input.actions["MoveDirection"].WasReleasedThisFrame())
            timeAtReleasedInput = Time.time;

        if (inputDirection4Way == Vector2.zero && inputDirectionBuffered4Way != Vector2.zero && Time.time >= timeAtReleasedInput + INPUT_BUFFER_DURATION)
            inputDirectionBuffered4Way = Vector2.zero;
    }
}
