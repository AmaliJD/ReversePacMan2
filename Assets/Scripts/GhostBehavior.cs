using EX;
using System.Linq;
using UnityEngine;
using static MovementController;

public class GhostBehavior : MonoBehaviour
{
    public enum GhostState
    {
        Wait, Chase, Scared, Scatter, Eaten
    }
    public GhostState state;
    GhostState prevState;

    Vector2 startPosition;
    bool isHome;

    const float WAIT_DURATION = 3;
    float timeEnteredHome;

    MovementController movementController;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        timeEnteredHome = Time.time;
        startPosition = transform.position.Round();
        prevState = state;
    }

    private void Start()
    {
        Static.main.AddGhost(this);
    }

    void CheckIsHome() => isHome = Static.main.homeCells.Select(x => x.transform.position.Round()).Contains(transform.position.Round());

    MovementBehaviorStruct waitingBehavior = new MovementBehaviorStruct()
    {
        hitWallBehavior = HitWallBehavior.TurnStop,
        setDirectionBehavior = SetDirectionBehavior.Auto,
        reverseInputBehavior = ReverseInputBehavior.None,
        countIgnoreWalls = true,
        target = null,
        speed = 5
    };

    MovementBehaviorStruct chasingBehavior = new MovementBehaviorStruct()
    {
        hitWallBehavior = HitWallBehavior.TurnStop,
        setDirectionBehavior = SetDirectionBehavior.Target,
        reverseInputBehavior = ReverseInputBehavior.None,
        countIgnoreWalls = true,
        target = null,
        speed = 5
    };

    MovementBehaviorStruct eatenBehavior = new MovementBehaviorStruct()
    {
        hitWallBehavior = HitWallBehavior.TurnStop,
        setDirectionBehavior = SetDirectionBehavior.Target,
        reverseInputBehavior = ReverseInputBehavior.None,
        countIgnoreWalls = false,
        target = null,
        speed = 15
    };

    public void BehaviorUpdate()
    {
        if (!TileMapProcessor._wallTileMap.cellBounds.ExtendBounds(2, 2).Contains(new Vector3Int(transform.position.x.RoundToInt(), transform.position.y.RoundToInt(), 0)))
        {
            transform.position = startPosition;
            timeEnteredHome = Time.time;
            state = GhostState.Wait;
            movementController.ResetMoveDirection();
        }

        CheckIsHome();

        switch (state)
        {
            case GhostState.Wait:
                movementController.SetMovementParameters(waitingBehavior);

                if (Time.time >= timeEnteredHome + WAIT_DURATION)
                    state = GhostState.Chase;
                break;

            case GhostState.Chase:
                chasingBehavior.target = Static.main.gakMen.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToArray()[0].transform;
                chasingBehavior.countIgnoreWalls = !isHome;
                movementController.SetMovementParameters(chasingBehavior);
                break;

            case GhostState.Eaten:
                if (prevState != GhostState.Eaten)
                {
                    eatenBehavior.target = Static.main.homeCells.Select(x => x.transform).ToArray()[UnityEngine.Random.Range(0, Static.main.homeCells.Count)].transform;
                }
                movementController.SetMovementParameters(eatenBehavior);

                if (isHome)
                {
                    state = GhostState.Wait;
                    timeEnteredHome = Time.time;
                }
                break;
        }

        prevState = state;
    }
}
