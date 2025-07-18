using EX;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterMovement : MonoBehaviour
{
    [Min(0)]
    public float speed = 1;
    float moveMultiplier = 1;

    Vector2 prevGridPos;
    Vector2 nextGridPos;
    Vector2 targetPos;
    Vector2 inputDirection;
    Vector2 moveDirection;

    public enum HitWallBehavior { Stop, TurnStop, TurnReverse, Reverse }
    public HitWallBehavior hitWallBehavior;

    public enum SetDirectionBehavior { Manual, Input, Target}
    public SetDirectionBehavior setDirectionBehavior;
    public Transform target;
    //public bool readMoveInput;

    [Range(0, 1)]
    public float inputPostBuffer;

    public enum ReverseInputBehavior { None, Anytime, NodeOnly, WallOnly }
    public ReverseInputBehavior reverseInputBehavior;

    private void Awake()
    {
        moveDirection = Vector2.zero;
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
        targetPos = target.position;

        // read input
        switch (setDirectionBehavior)
        {
            case SetDirectionBehavior.Input:
                inputDirection = InputProcessor.inputDirection4Way;
                break;
            case SetDirectionBehavior.Target:
                // error if target is null
                if (target == null)
                {
                    inputDirection = Vector2.zero;
                    break;
                }

                Vector2 nextTransformPosition = Vector2.MoveTowards(transform.position, nextGridPos, speed.PerFrame());
                bool atNode = nextTransformPosition == nextGridPos && TileMapProcessor.nodePositions.Contains(transform.position.Round());
                bool atWall = atNode && TileMapProcessor.HasTile(nextGridPos + moveDirection, true);

                // if can reverse anytime and not at node
                if (!atNode && reverseInputBehavior == ReverseInputBehavior.Anytime)
                {
                    float nextNodeDistanceToTarget = Vector2.Distance(nextGridPos, targetPos);
                    float prevNodeDistanceToTarget = Vector2.Distance(prevGridPos, targetPos) * 2;

                    if (prevNodeDistanceToTarget < nextNodeDistanceToTarget)
                    {
                        inputDirection = (prevGridPos - nextGridPos).normalized;
                        break;
                    }
                }

                // at node calculate
                if (!atNode)
                    break;

                bool reverseDirection = reverseInputBehavior switch
                {
                    ReverseInputBehavior.None => false,
                    ReverseInputBehavior.Anytime => true,
                    ReverseInputBehavior.NodeOnly => atNode,
                    ReverseInputBehavior.WallOnly => atWall,
                    _ => false
                };

                List<Vector2> directions = new();
                Vector2 testDirection = Vector2.up;
                Vector2 reverseTestDirection = Vector2.zero;
                for (int i = 0; i < 4; i++)
                {
                    if (!TileMapProcessor.HasTile(nextGridPos + testDirection, true))
                    {
                        if (testDirection == -moveDirection ? reverseDirection : true)
                        {
                            directions.Add(testDirection);

                            if (testDirection == -moveDirection)
                                reverseTestDirection = testDirection;
                        }
                    }

                    testDirection = testDirection.Rotate90CCW();
                }

                directions = directions.OrderBy(x => Vector2.Distance(nextGridPos + x * (x == reverseTestDirection ? 2 : 1), targetPos)).ToList();

                if (directions.Count > 0)
                    inputDirection = directions[0];
                else
                    inputDirection = Vector2.zero;

                break;
            case SetDirectionBehavior.Manual:
                inputDirection = Vector2.zero;
                break;
        }

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

        // at teleport
        if (Static.main.teleportReferences.ContainsKey(transform.position))
        {
            transform.position = Static.main.teleportReferences.GetValueOrDefault(transform.position);
        }

        prevGridPos = transform.position.Round();
        transform.position = prevGridPos;

        // at node
        if (TileMapProcessor.nodePositions.Contains(transform.position))
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
