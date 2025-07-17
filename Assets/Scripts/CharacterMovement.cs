using EX;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Min(0)]
    public float speed = 1;
    float moveMultiplier = 1;

    Vector2 prevGridPos;
    Vector2 nextGridPos;
    Vector2 inputDirection;
    Vector2 moveDirection;
    Vector2 targetPos;

    public enum HitWallBehavior { Stop, TurnStop, TurnReverse, Reverse }
    public HitWallBehavior hitWallBehavior;

    public bool readMoveInput;

    [Range(0, 1)]
    public float inputPostBuffer;

    public enum ReverseInputBehavior { None, Anytime, NodeOnly, WallOnly }
    public ReverseInputBehavior reverseInputBehavior;

    private void Awake()
    {
        moveDirection = Vector2.right;
        prevGridPos = transform.position.Round();
        nextGridPos = prevGridPos + moveDirection;

        transform.position = prevGridPos;
    }

    private void Start()
    {
        Static.main.AddCharacter(this);
    }

    public void Move()
    {
        // read input
        if (readMoveInput)
            inputDirection = InputProcessor.inputDirection4Way;
        else
            inputDirection = Vector2.zero;

        // reverse movement
        if (reverseInputBehavior == ReverseInputBehavior.Anytime && inputDirection == -moveDirection)
        {
            moveDirection = inputDirection;
            moveMultiplier = 1;

            prevGridPos = nextGridPos;
            nextGridPos = prevGridPos + moveDirection * moveMultiplier;
        }

        // move player
        transform.position = Vector2.MoveTowards(transform.position, nextGridPos, speed.PerFrame());

        bool withinPostBuffer = inputPostBuffer > 0 && Vector2.Distance((Vector2)transform.position, prevGridPos) <= Vector2.Distance(prevGridPos + moveDirection * inputPostBuffer, prevGridPos) && Vector2.Dot((Vector2)transform.position - prevGridPos, moveDirection * inputPostBuffer) > 0;
        bool readInput = InputProcessor.inputDirectionPressedThisFrame && inputDirection != Vector2.zero && inputDirection != -moveDirection && inputDirection != moveDirection && !TileMapProcessor.HasTile(prevGridPos + inputDirection, true);

        // skip input
        if (!((Vector2)transform.position == nextGridPos || (withinPostBuffer && readInput)))
            return;

        if (withinPostBuffer && readInput)
        {
            transform.position = prevGridPos;
            nextGridPos = prevGridPos;
        }

        prevGridPos = transform.position.Round();
        transform.position = prevGridPos;

        // at node
        if (TileMapProcessor.positions.Contains(transform.position))
        {
            if (inputDirection != Vector2.zero &&
                reverseInputBehavior switch
                {
                    ReverseInputBehavior.NodeOnly => true,
                    ReverseInputBehavior.WallOnly => inputDirection == -moveDirection ? TileMapProcessor.HasTile(prevGridPos + moveDirection, true) : true,
                    _ => inputDirection != -moveDirection
                } &&
                !TileMapProcessor.HasTile(prevGridPos + inputDirection, true))
            {
                moveDirection = inputDirection;
                moveMultiplier = 1;
            }

            // at wall
            if (TileMapProcessor.HasTile(prevGridPos + moveDirection, true))
            {
                Vector2 TurnMoveDirection()
                {
                    if (!TileMapProcessor.HasTile(prevGridPos + moveDirection.Rotate90CCW(), true))
                        return moveDirection.Rotate90CCW();

                    if (!TileMapProcessor.HasTile(prevGridPos + moveDirection.Rotate90CW(), true))
                        return moveDirection.Rotate90CW();

                    return hitWallBehavior switch
                    {
                        HitWallBehavior.Stop => moveDirection,
                        HitWallBehavior.Reverse => -moveDirection,
                        HitWallBehavior.TurnStop => moveDirection,
                        HitWallBehavior.TurnReverse => -moveDirection,
                        _ => Vector2.zero
                    };
                }

                Vector2 prevMoveDirection = moveDirection;

                moveDirection = hitWallBehavior switch
                {
                    HitWallBehavior.Stop => moveDirection,
                    HitWallBehavior.Reverse => -moveDirection,
                    HitWallBehavior.TurnStop => TurnMoveDirection(),
                    HitWallBehavior.TurnReverse => TurnMoveDirection(),
                    _ => Vector2.zero
                };

                moveMultiplier = (moveDirection == prevMoveDirection) ? 0 : 1;
            }
        }

        nextGridPos = prevGridPos + moveDirection * moveMultiplier;
    }
}
