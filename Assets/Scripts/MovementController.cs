using EX;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MovementController : MonoBehaviour
{
    [Min(0)]
    public float speed = 1;
    float moveMultiplier = 1;

    Vector2 prevGridPos;
    Vector2 nextGridPos;
    Vector2 inputDirection;
    Vector2 moveDirection;
    Vector2 lastNodeTouched;

    public Vector2 GetNextGridPos() => nextGridPos;
    public Vector2 GetPrevGridPos() => prevGridPos;
    public Vector2 GetMoveDirection() => moveDirection;

    public enum HitWallBehavior { Stop, TurnStop, TurnReverse, Reverse }
    public HitWallBehavior hitWallBehavior;

    public enum SetDirectionBehavior { Manual, Auto, Input, Target, Random}
    public SetDirectionBehavior setDirectionBehavior;
    public Transform target;
    public Vector2 targetOffset;

    [Range(0, 1)]
    public float inputPostBuffer;

    public enum ReverseInputBehavior { None, Anytime, NodeOnly, WallOnly }
    public ReverseInputBehavior reverseInputBehavior;

    public bool countIgnoreWalls;
    public bool countSecretWalls;

    public struct MovementBehaviorStruct
    {
        public HitWallBehavior hitWallBehavior;
        public SetDirectionBehavior setDirectionBehavior;
        public ReverseInputBehavior reverseInputBehavior;
        public bool countIgnoreWalls;
        public Transform target;
        public Vector2 targetOffset;
        public float speed;
    }

    private void Awake()
    {
        moveDirection = Vector2.zero;

        prevGridPos = transform.position.Round();
        nextGridPos = prevGridPos + moveDirection;
        lastNodeTouched = prevGridPos;

        transform.position = prevGridPos;

        Static.main.AddMovementController(this);
    }

    public MovementBehaviorStruct GetMovementParameters()
    {
        return new MovementBehaviorStruct
        {
            hitWallBehavior = hitWallBehavior,
            setDirectionBehavior = setDirectionBehavior,
            reverseInputBehavior = reverseInputBehavior,
            countIgnoreWalls = countIgnoreWalls,
            target = target,
            targetOffset = targetOffset,
            speed = speed
        };
    }

    public void SetMovementParameters(MovementBehaviorStruct movementBehavior)
    {
        if (GetMovementParameters().Equals(movementBehavior))
            return;

        hitWallBehavior = movementBehavior.hitWallBehavior;
        setDirectionBehavior = movementBehavior.setDirectionBehavior;
        reverseInputBehavior = movementBehavior.reverseInputBehavior;
        countIgnoreWalls = movementBehavior.countIgnoreWalls;
        target = movementBehavior.target;
        targetOffset = movementBehavior.targetOffset;
        speed = movementBehavior.speed;
    }

    public void ResetMoveDirection()
    {
        moveDirection = Vector2.zero;
        inputDirection = Vector2.zero;
        prevGridPos = transform.position.Round();
        nextGridPos = prevGridPos + moveDirection;
    }

    public void InstantReverseMoveDirection()
    {
        if (TileMapProcessor.nodePositions.Contains(transform.position) && TileMapProcessor.HasTile(prevGridPos - moveDirection, countIgnoreWalls, countSecretWalls))
            return;

        moveDirection = -moveDirection;
        moveMultiplier = 1;

        prevGridPos = nextGridPos;
        nextGridPos = prevGridPos + moveDirection * moveMultiplier;
    }

    public void TeleportSetMoveDirection(Vector2 direction)
    {
        moveDirection = direction;
        moveMultiplier = 1;

        prevGridPos = transform.position.Round();
        nextGridPos = prevGridPos + moveDirection * moveMultiplier;
    }

    List<Vector2> GetAvailableDirections(bool reverseDirection)
    {
        List<Vector2> directions = new();
        Vector2 testDirection = Vector2.up;
        Vector2 reverseTestDirection = Vector2.zero;
        for (int i = 0; i < 4; i++)
        {
            if (!TileMapProcessor.HasTile(nextGridPos + testDirection, countIgnoreWalls, countSecretWalls))
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

        return directions;
    }

    public void Move()
    {
        // read input
        switch (setDirectionBehavior)
        {
            case SetDirectionBehavior.Input:
                inputDirection = InputProcessor.inputDirectionBuffered4Way;
                break;

            case SetDirectionBehavior.Target:
                // error if target is null
                if (target == null)
                {
                    inputDirection = Vector2.zero;
                    break;
                }

                Vector2 nextTransformPosition = Vector2.MoveTowards(transform.position, nextGridPos, speed.PerFrame() * Static.main.gameSpeed);
                bool atNode = nextTransformPosition == nextGridPos && TileMapProcessor.nodePositions.Contains(transform.position.Round());
                bool atWall = atNode && TileMapProcessor.HasTile(nextGridPos + moveDirection, countIgnoreWalls, countSecretWalls);

                // if can reverse anytime and not at node
                if (!atNode && reverseInputBehavior == ReverseInputBehavior.Anytime)
                {
                    float nextNodeDistanceToTarget = Vector2.Distance(nextGridPos, (Vector2)target.position + targetOffset);
                    float prevNodeDistanceToTarget = Vector2.Distance(prevGridPos, (Vector2)target.position + targetOffset) * 2;

                    if (prevNodeDistanceToTarget < nextNodeDistanceToTarget)
                    {
                        inputDirection = (prevGridPos - nextGridPos).normalized;
                        break;
                    }
                }

                // at node calculate
                if (!atNode && moveDirection != Vector2.zero)
                    break;

                bool reverseDirection = reverseInputBehavior switch
                {
                    ReverseInputBehavior.None => false,
                    ReverseInputBehavior.Anytime => true,
                    ReverseInputBehavior.NodeOnly => atNode,
                    ReverseInputBehavior.WallOnly => atWall,
                    _ => false
                };

                List<Vector2> openDirections = new();
                Vector2 testDirection = Vector2.up;
                Vector2 reverseTestDirection = Vector2.zero;
                for (int i = 0; i < 4; i++)
                {
                    if (!TileMapProcessor.HasTile(nextGridPos + testDirection, countIgnoreWalls, countSecretWalls))
                    {
                        if (testDirection == -moveDirection ? reverseDirection : true)
                        {
                            openDirections.Add(testDirection);

                            if (testDirection == -moveDirection)
                                reverseTestDirection = testDirection;
                        }
                    }

                    testDirection = testDirection.Rotate90CCW();
                }

                openDirections = openDirections.OrderBy(x => Vector2.Distance(nextGridPos + x * (x == reverseTestDirection ? 2 : 1), (Vector2)target.position + targetOffset)).ToList();

                if (openDirections.Count > 0)
                    inputDirection = openDirections[0];
                else
                    inputDirection = Vector2.zero;

                break;

            case SetDirectionBehavior.Auto:
                if (moveDirection == Vector2.zero)
                    inputDirection = Vector2.up.Rotate(UnityEngine.Random.Range(0, 4) * 90);
                else if (inputDirection != Vector2.zero)
                    inputDirection = Vector2.zero;
                break;

            case SetDirectionBehavior.Random:
                nextTransformPosition = Vector2.MoveTowards(transform.position, nextGridPos, speed.PerFrame() * Static.main.gameSpeed);
                atNode = nextTransformPosition == nextGridPos && TileMapProcessor.nodePositions.Contains(transform.position.Round());
                atWall = atNode && TileMapProcessor.HasTile(nextGridPos + moveDirection, countIgnoreWalls, countSecretWalls);

                reverseDirection = reverseInputBehavior switch
                {
                    ReverseInputBehavior.None => false,
                    ReverseInputBehavior.Anytime => true,
                    ReverseInputBehavior.NodeOnly => atNode,
                    ReverseInputBehavior.WallOnly => atWall,
                    _ => false
                };

                openDirections = GetAvailableDirections(reverseDirection);

                if (openDirections.Count > 0)
                    inputDirection = openDirections[UnityEngine.Random.Range(0, openDirections.Count)];
                else
                    inputDirection = Vector2.up.Rotate(UnityEngine.Random.Range(0, 4) * 90);
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
        transform.position = Vector2.MoveTowards(transform.position, nextGridPos, speed.PerFrame() * Static.main.gameSpeed);

        bool withinPostBuffer = inputPostBuffer > 0 && Vector2.Distance((Vector2)transform.position, prevGridPos) <= Vector2.Distance(prevGridPos + moveDirection * inputPostBuffer, prevGridPos) && Vector2.Dot((Vector2)transform.position - prevGridPos, moveDirection * inputPostBuffer) > 0;
        bool readInput = InputProcessor.inputDirectionPressedThisFrame && inputDirection != Vector2.zero && inputDirection != -moveDirection && inputDirection != moveDirection && !TileMapProcessor.HasTile(prevGridPos + inputDirection, countIgnoreWalls, countSecretWalls);

        // skip input
        if (!((Vector2)transform.position == nextGridPos || (withinPostBuffer && readInput)))
            return;

        if (withinPostBuffer && readInput)
        {
            transform.position = prevGridPos;
            nextGridPos = prevGridPos;
        }

        // at teleport
        if (Static.main.teleportalPairs.ContainsKey(transform.position))
        {
            Vector2[] teleportPositionDirection = Static.main.teleportalPairs.GetValueOrDefault(transform.position);

            transform.position = teleportPositionDirection[0].Round();
            TeleportSetMoveDirection(teleportPositionDirection[1]);
        }

        prevGridPos = transform.position.Round();
        transform.position = prevGridPos;
        lastNodeTouched = prevGridPos;

        // at node
        if (TileMapProcessor.nodePositions.Contains(transform.position) || moveDirection == Vector2.zero)
        {
            if (inputDirection != Vector2.zero &&
                reverseInputBehavior switch
                {
                    ReverseInputBehavior.NodeOnly => true,
                    ReverseInputBehavior.WallOnly => inputDirection == -moveDirection ? TileMapProcessor.HasTile(prevGridPos + moveDirection, countIgnoreWalls, countSecretWalls) : true,
                    _ => inputDirection != -moveDirection
                } &&
                !TileMapProcessor.HasTile(prevGridPos + inputDirection, countIgnoreWalls, countSecretWalls))
            {
                moveDirection = inputDirection;
                moveMultiplier = 1;
            }

            // at wall
            if (TileMapProcessor.HasTile(prevGridPos + moveDirection, countIgnoreWalls, countSecretWalls))
            {
                Vector2 TurnMoveDirection()
                {
                    if (!TileMapProcessor.HasTile(prevGridPos + moveDirection.Rotate90CCW(), countIgnoreWalls, countSecretWalls))
                        return moveDirection.Rotate90CCW();

                    if (!TileMapProcessor.HasTile(prevGridPos + moveDirection.Rotate90CW(), countIgnoreWalls, countSecretWalls))
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
