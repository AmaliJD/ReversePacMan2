using EX;
using GLG;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using static MovementController;
using PrimeTween;
using UnityEngine.Assertions.Must;

[RequireComponent(typeof(MovementController))]
public class GhostBehavior : MonoBehaviour
{
    public enum GhostState
    {
        Wait, Chase, Scared, Scatter, Eaten
    }
    public GhostState state;
    GhostState prevState;

    public enum GhostType
    {
        Red, Purple, Pink, Green, Orange, Yellow, White, Custom, Cyan
    }
    public GhostType ghostType;

    public static GhostBehavior leadGhost;

    [Min(0)]
    float gakManRadius;

    Vector2 startPosition;
    bool isHome;
    bool prevIsHome;
    public bool IsHome() => isHome;

    float waitDuration = 5;
    float timeEnteredHome;

    float scaredDuration = 7;
    float timeBecameScared;

    public Transform scatterTarget;
    public int eyeSpriteIndex;

    MovementController movementController;
    Color ghostColor, eyeColor;
    SpriteRenderer bodySprite, eyeSprite;
    Sequence bodyFlash, eyeFlash;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        timeEnteredHome = Time.time;
        startPosition = transform.position.Round();
        prevState = state;
        InitGhost();

        Static.main.AddGhost(this);
    }

    private void Start()
    {
        if (ghostType == GhostType.White)
        {
            scaredBehavior.hitWallBehavior = inputChasingBehavior.hitWallBehavior;
            scaredBehavior.setDirectionBehavior = inputChasingBehavior.setDirectionBehavior;
            scaredBehavior.reverseInputBehavior = inputChasingBehavior.reverseInputBehavior;
        }
    }

    void InitGhost()
    {
        switch (ghostType)
        {
            case GhostType.Red:
                ghostColor = Color.red;
                eyeColor = Color.white;
                gakManRadius = 0;
                break;

            case GhostType.Pink:
                ghostColor = new Color(1, .72f, 1);
                eyeColor = Color.white;
                gakManRadius = 0;
                break;

            case GhostType.Cyan:
                ghostColor = Color.cyan;
                eyeColor = Color.white;
                gakManRadius = 0;
                break;

            case GhostType.Orange:
                ghostColor = new Color(1, .67f, .23f);
                eyeColor = Color.white;
                gakManRadius = 8;
                break;

            case GhostType.White:
                ghostColor = Color.white;
                eyeColor = Color.red;
                gakManRadius = 0;
                break;

            case GhostType.Green:
                ghostColor = new Color(.25f, 1, 0);
                eyeColor = new Color(.94f, 1f, 1f);
                gakManRadius = 0;
                break;

            case GhostType.Purple:
                ghostColor = new Color(.53f, .33f, .9f);
                eyeColor = new Color(1, .85f, .85f);
                gakManRadius = 0;
                break;

            case GhostType.Yellow:
                ghostColor = new Color(1, 0.88f, 0);
                eyeColor = Color.white;
                gakManRadius = 8;
                break;
        }

        bodySprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        bodySprite.color = ghostColor;

        eyeSprite = transform.GetChild(1).GetComponent<SpriteRenderer>();
        eyeSprite.color = eyeColor;
    }

    void CheckIsHome() => isHome = Static.main.homeCells.Select(x => x.transform.position.Round()).Contains(transform.position.Round());

    MovementBehaviorStruct inputChasingBehavior = new MovementBehaviorStruct()
    {
        hitWallBehavior = HitWallBehavior.Stop,
        setDirectionBehavior = SetDirectionBehavior.Input,
        reverseInputBehavior = ReverseInputBehavior.WallOnly,
        countIgnoreWalls = true,
        target = null,
        targetOffset = Vector2.zero,
        speed = 5
    };

    MovementBehaviorStruct waitingBehavior = new MovementBehaviorStruct()
    {
        hitWallBehavior = HitWallBehavior.TurnReverse,
        setDirectionBehavior = SetDirectionBehavior.Auto,
        reverseInputBehavior = ReverseInputBehavior.None,
        countIgnoreWalls = true,
        target = null,
        targetOffset = Vector2.zero,
        speed = 4
    };

    MovementBehaviorStruct chasingBehavior = new MovementBehaviorStruct()
    {
        hitWallBehavior = HitWallBehavior.TurnReverse,
        setDirectionBehavior = SetDirectionBehavior.Target,
        reverseInputBehavior = ReverseInputBehavior.None,
        countIgnoreWalls = true,
        target = null,
        targetOffset = Vector2.zero,
        speed = 5
    };

    MovementBehaviorStruct eatenBehavior = new MovementBehaviorStruct()
    {
        hitWallBehavior = HitWallBehavior.TurnReverse,
        setDirectionBehavior = SetDirectionBehavior.Target,
        reverseInputBehavior = ReverseInputBehavior.None,
        countIgnoreWalls = false,
        target = null,
        targetOffset = Vector2.zero,
        speed = 15
    };

    MovementBehaviorStruct scaredBehavior = new MovementBehaviorStruct()
    {
        hitWallBehavior = HitWallBehavior.TurnReverse,
        setDirectionBehavior = SetDirectionBehavior.Random,
        reverseInputBehavior = ReverseInputBehavior.None,
        countIgnoreWalls = true,
        target = null,
        targetOffset = Vector2.zero,
        speed = 2.5f
    };

    MovementBehaviorStruct scatterBehavior = new MovementBehaviorStruct()
    {
        hitWallBehavior = HitWallBehavior.TurnReverse,
        setDirectionBehavior = SetDirectionBehavior.Target,
        reverseInputBehavior = ReverseInputBehavior.None,
        countIgnoreWalls = true,
        target = null,
        targetOffset = Vector2.zero,
        speed = 5
    };

    (Transform, Vector2) GetChaseTarget()
    {
        Transform target = null;
        Vector2 offset = Vector2.zero;

        switch (ghostType)
        {
            case GhostType.Red:
                target = Static.main.gakMen.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToArray()[0].transform;
                break;

            case GhostType.Pink:
                target = Static.main.gakMen.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToArray()[0].transform;
                MovementController gakManController = target.GetComponent<MovementController>();
                offset = (4 * gakManController.GetMoveDirection().normalized);// - ((Vector2)gakManController.transform.position - gakManController.GetPrevGridPos());
                break;

            case GhostType.Cyan:
                target = Static.main.gakMen.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToArray()[0].transform;
                gakManController = target.GetComponent<MovementController>();
                offset = (2 * gakManController.GetMoveDirection().normalized);
                offset += (Vector2)(gakManController.transform.position - leadGhost.transform.position);
                break;

            case GhostType.Orange:
                target = Static.main.gakMen.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToArray()[0].transform;

                if (Vector2.Distance(target.position, transform.position) < gakManRadius)
                {
                    if (scatterTarget == null && Static.main.scatterTargets.Count > 0)
                        scatterTarget = Static.main.scatterTargets[0].transform;

                    target = scatterTarget;
                }
                break;

            case GhostType.Green:
                target = transform;
                List<Vector2> positions = Static.main.gakMen.Select(x => (Vector2)x.transform.position).ToList().Concat(Static.main.ghosts.Select(x => (Vector2)x.transform.position)).ToList();
                Vector2 averagePosition = Vector2.zero;
                foreach (Vector2 position in positions)
                    averagePosition += position;
                averagePosition /= positions.Count;
                offset = averagePosition - (Vector2)transform.position;
                break;

            case GhostType.Purple:
                target = Static.main.gakMen.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToArray()[0].transform;
                chasingBehavior.reverseInputBehavior = ReverseInputBehavior.WallOnly;
                break;

            case GhostType.Yellow:
                target = Static.main.gakMen.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToArray()[0].transform;

                if (Vector2.Distance(target.position, transform.position) <= gakManRadius)
                {
                    chasingBehavior.setDirectionBehavior = SetDirectionBehavior.Target;
                    chasingBehavior.speed = 5.5f;
                }
                else
                {
                    chasingBehavior.setDirectionBehavior = SetDirectionBehavior.Random;
                    chasingBehavior.speed = isHome ? 5 : 2.5f;
                }
                break;
        }

        return (target, offset);
    }

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

                if (Time.time >= timeEnteredHome + waitDuration)
                    state = Static.main.scatterMode ? GhostState.Scatter : GhostState.Chase;
                break;

            case GhostState.Chase:
                if (ghostType != GhostType.White || isHome)
                {
                    (Transform target, Vector2 offset) = GetChaseTarget();

                    if (!isHome)
                    {
                        chasingBehavior.target = target;
                        chasingBehavior.targetOffset = offset;
                    }
                    else
                    {
                        chasingBehavior.target = Static.main.homeCellEntrances.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToArray()[0].transform;
                        chasingBehavior.targetOffset = Vector2.zero;
                    }

                    chasingBehavior.countIgnoreWalls = !isHome;
                    movementController.SetMovementParameters(chasingBehavior);
                }
                else
                {
                    movementController.SetMovementParameters(inputChasingBehavior);
                }
                break;

            case GhostState.Eaten:
                if (prevState != GhostState.Eaten)
                {
                    eatenBehavior.target = Static.main.homeCellEntrances.Select(x => x.transform).ToArray()[UnityEngine.Random.Range(0, Static.main.homeCellEntrances.Count)].transform;
                }
                movementController.SetMovementParameters(eatenBehavior);

                if (isHome)
                {
                    state = GhostState.Wait;
                    timeEnteredHome = Time.time;
                }
                break;

            case GhostState.Scared:
                if (prevState != GhostState.Scared)
                {
                    movementController.InstantReverseMoveDirection();
                    timeBecameScared = Time.time;
                }
                movementController.SetMovementParameters(scaredBehavior);

                if (Time.time >= timeBecameScared + scaredDuration)
                {
                    //movementController.InstantReverseMoveDirection();
                    state = Static.main.scatterMode ? GhostState.Scatter : GhostState.Chase;
                }
                break;

            case GhostState.Scatter:
                if (ghostType == GhostType.White)
                {
                    state = GhostState.Chase;
                    break;
                }

                if (isHome)
                {
                    scatterBehavior.target = Static.main.homeCellEntrances.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToArray()[0].transform;
                    scatterBehavior.setDirectionBehavior = SetDirectionBehavior.Target;
                }
                else
                {
                    if (prevState != GhostState.Scatter)
                    {
                        movementController.InstantReverseMoveDirection();
                        scatterBehavior.target = scatterTarget;

                        if (scatterTarget != null)
                            scatterBehavior.setDirectionBehavior = SetDirectionBehavior.Target;
                        else
                            scatterBehavior.setDirectionBehavior = SetDirectionBehavior.Random;
                    }
                    else if (prevIsHome && !isHome)
                    {
                        scatterBehavior.target = scatterTarget;

                        if (scatterTarget != null)
                            scatterBehavior.setDirectionBehavior = SetDirectionBehavior.Target;
                        else
                            scatterBehavior.setDirectionBehavior = SetDirectionBehavior.Random;
                    }
                }

                if (ghostType == GhostType.Yellow)
                    scatterBehavior.speed = isHome ? 5 : 2.5f;

                scatterBehavior.countIgnoreWalls = !isHome;
                movementController.SetMovementParameters(scatterBehavior);
                break;
        }

        Visuals();

        prevState = state;
        prevIsHome = isHome;
    }

    public void ResetScareTime() => timeBecameScared = Time.time;

    void Visuals()
    {
        if (movementController.GetMoveDirection() == Vector2.right && transform.localScale.x != -1)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (movementController.GetMoveDirection() == Vector2.left && transform.localScale.x !=     1)
            transform.localScale = new Vector3(1, 1, 1);

        if (Static.main.ghostSprites.Count == 0)
            return;

        if (state == GhostState.Scared)
        {
            eyeSprite.sprite = Static.main.ghostSprites[5];
            eyeSprite.color = Color.red;
            bodySprite.color = Color.blue;

            if (prevState != GhostState.Scared)
            {
                float randomStart = UnityEngine.Random.Range(0, .6f);
                Invoke("StartBodyFlashing", randomStart);
                Invoke("StartEyeFlashing", randomStart + .4f);
            }
            
        }
        else
        {
            if (prevState == GhostState.Scared)
            {
                bodyFlash.Stop();
                eyeFlash.Stop();
            }

            eyeSprite.sprite = movementController.GetMoveDirection().y switch
            {
                1 => Static.main.ghostSprites[eyeSpriteIndex + 2],
                -1 => Static.main.ghostSprites[eyeSpriteIndex + 1],
                _ => Static.main.ghostSprites[eyeSpriteIndex],
            };
            eyeSprite.color = eyeColor;
            bodySprite.color = state == GhostState.Eaten ? ghostColor.SetAlpha(.025f) : ghostColor;
        }
    }

    void StartBodyFlashing()
    {
        bodyFlash = Sequence.Create()
                    .Chain(Tween.Color(bodySprite, Color.white, .05f))
                    .Chain(Tween.Color(bodySprite, Color.blue, 1f))
                    .OnComplete(() => Invoke("StartBodyFlashing", 0));
    }

    void StartEyeFlashing()
    {
        eyeFlash = Sequence.Create()
                    .Chain(Tween.Color(eyeSprite, Color.white, .05f))
                    .Chain(Tween.Color(eyeSprite, Color.red, 1f))
                    .OnComplete(() => Invoke("StartEyeFlashing", 0));
    }

    public void Gizmos()
    {
        switch (state)
        {
            case GhostState.Chase:
                GLGizmos.SetColor(ghostColor.SetAlpha(.75f));
                if (movementController.target != null)
                {
                    if (chasingBehavior.setDirectionBehavior != SetDirectionBehavior.Target)
                    {
                        GLGizmos.DrawLine(transform.position, movementController.GetNextGridPos());
                        GLGizmos.DrawSolidCircle(movementController.GetNextGridPos(), .1f);
                    }
                    else
                    {
                        GLGizmos.DrawLine(transform.position, (Vector2)movementController.target.position + movementController.targetOffset);
                        GLGizmos.DrawSolidCircle((Vector2)movementController.target.position + movementController.targetOffset, .1f);
                    }

                    if (gakManRadius > 0 && !isHome)
                    {
                        GLGizmos.SetColor(ghostColor.SetAlpha(.25f));
                        foreach (GakManBehavior gakMan in Static.main.gakMen)
                        {
                            GLGizmos.DrawOpenCircle(gakMan.transform.position, gakManRadius);
                        }
                    }
                }
                else
                {
                    GLGizmos.DrawLine(transform.position, movementController.GetNextGridPos());
                    GLGizmos.DrawSolidCircle(movementController.GetNextGridPos(), .1f);
                }
                break;

            case GhostState.Scatter:
                GLGizmos.SetColor(ghostColor.SetAlpha(.75f));
                if (scatterTarget != null)
                {
                    GLGizmos.DrawLine(transform.position, scatterTarget.position);
                    GLGizmos.DrawSolidCircle(scatterTarget.position, .1f);
                }
                else
                {
                    GLGizmos.DrawLine(transform.position, movementController.GetNextGridPos());
                    GLGizmos.DrawSolidCircle(movementController.GetNextGridPos(), .1f);
                }
                break;

            case GhostState.Eaten:
                GLGizmos.SetColor(ghostColor.SetAlpha(.75f));
                GLGizmos.DrawLine(transform.position, movementController.target.position);
                GLGizmos.DrawSolidCircle(movementController.target.position, .1f);
                break;

            case GhostState.Wait:
            case GhostState.Scared:
                GLGizmos.SetColor(ghostColor.SetAlpha(.75f));
                GLGizmos.DrawLine(transform.position, movementController.GetNextGridPos());
                GLGizmos.DrawSolidCircle(movementController.GetNextGridPos(), .1f);
                break;
        }
    }
}
