using UnityEngine;
using UnityEngine.Tilemaps;
using GLG;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using EX;
using System.Linq;
using PrimeTween;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public Tilemap WallTilemap;
    public Tilemap SecretWallTilemap;
    List<MovementController> movementCtrlrs = new();
    
    [HideInInspector]
    public Dictionary<Vector2, Vector2[]> teleportalPairs = new();
    List<TeleportReference> teleportReferences = new();

    [HideInInspector]
    public List<HomeCell> homeCells = new();
    [HideInInspector]
    public List<HomeCell> homeCellEntrances = new();
    [HideInInspector]
    public List<GhostBehavior> ghosts = new();
    [HideInInspector]
    public List<GakManBehavior> gakMen = new();
    [HideInInspector]
    public List<ScatterTarget> scatterTargets = new();
    [HideInInspector]
    public List<Egg> eggs = new();
    [HideInInspector]
    public List<Egg> activeEggs = new();
    [HideInInspector]
    public List<Vector2> eggPositions = new();
    //[HideInInspector]
    public List<Sprite> ghostSprites;

    [Min(0)]
    public float gameSpeed = 1;
    public bool scatterMode;

    [Min(0)]
    public int eggsCollected;

    public bool drawGizmos = true;

    private void Awake()
    {
        Static.main = this;

        TileMapProcessor.Init(WallTilemap, SecretWallTilemap);
        TileMapProcessor.SearchTilemapForNodes();
        TileMapProcessor.RemoveIgnoreTiles();

        InputProcessor.input = GetComponent<PlayerInput>();
        
        SecretWallTilemap.color = WallTilemap.color.SetAlpha(.2f);
    }

    private void Start()
    {
        if (ghosts.Count > 0)
            GhostBehavior.leadGhost = ghosts.OrderBy(x => x.ghostType).ToArray()[0];
    }

    public void AddMovementController(MovementController character) => movementCtrlrs.Add(character);
    public void AddGhost(GhostBehavior ghost) => ghosts.Add(ghost);
    public void AddGakMen(GakManBehavior gakMan) => gakMen.Add(gakMan);
    public void AddScatterTargets(ScatterTarget st) => scatterTargets.Add(st);
    public void AddTPRef(TeleportReference tpr)
    {
        teleportalPairs.Add(tpr.transform.position.Round(), new Vector2[] { tpr.target.transform.position.Round(), tpr.target.transform.right });
        teleportReferences.Add(tpr);
    }
    public void AddHomeCell(HomeCell hc, bool isEntrance)
    {
        homeCells.Add(hc);

        if (isEntrance)
            homeCellEntrances.Add(hc);
    }
    public void AddEgg(Egg egg)
    {
        eggs.Add(egg);
        eggPositions.Add(egg.transform.position);
        //activeEggs.Add(egg);
    }

    private void Update()
    {
        InputProcessor.GetInputs();

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            drawGizmos = !drawGizmos;

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            foreach (GhostBehavior ghost in ghosts)
                ghost.state = GhostBehavior.GhostState.Eaten;
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            ScareGhosts();
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            if (!scatterMode)
                ScatterGhosts();
            else
                UnScatterGhosts();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ReloadScene();
        }

        // move all characters
        foreach (MovementController character in movementCtrlrs)
            character.Move();

        // update gakmen behavior
        foreach (GakManBehavior gakMan in gakMen)
            gakMan.BehaviorUpdate();

        // update ghost behavior
        foreach (GhostBehavior ghost in ghosts)
            ghost.BehaviorUpdate();

        // randomly rotate eggs
        if (eggs.Count > 0 && UnityEngine.Random.Range(0, 5500 / eggs.Count) == 0)
        {
            Transform eggTransform = eggs[UnityEngine.Random.Range(0, eggs.Count)].transform;
            Sequence.Create()
                .Group(Tween.EulerAngles(eggTransform, startValue: new Vector3(0, 0, 270), endValue: Vector2.zero, .5f, Ease.OutQuad))
                .Group(Tween.Scale(eggTransform, startValue: Vector3.one * 1.6f, endValue: Vector3.one, .5f, Ease.InOutBack));
        }

        if (drawGizmos)
            Gizmos();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ScatterGhosts()
    {
        scatterMode = true;
        scatterTargets = scatterTargets.Shuffle();
        List<GhostBehavior> activeGhosts = ghosts.Where(x => x.state == GhostBehavior.GhostState.Chase).ToList();

        if (activeGhosts.Count == 0)
            return;

        int i = 0;
        foreach (GhostBehavior ghost in activeGhosts)
        {
            if (scatterTargets.Count > 0)
                ghost.scatterTarget = scatterTargets[i].transform;

            i++;
            ghost.state = GhostBehavior.GhostState.Scatter;
        }
    }

    void UnScatterGhosts()
    {
        scatterMode = false;
        List<GhostBehavior> activeGhosts = ghosts.Where(x => x.state == GhostBehavior.GhostState.Scatter).ToList();

        if (activeGhosts.Count == 0)
            return;

        foreach (GhostBehavior ghost in activeGhosts)
        {
            ghost.state = GhostBehavior.GhostState.Chase;
        }
    }

    public void ScareGhosts()
    {
        List<GhostBehavior> activeGhosts = ghosts.Where(x => !x.IsHome() && x.state != GhostBehavior.GhostState.Eaten && x.state != GhostBehavior.GhostState.Wait).ToList();

        foreach (GhostBehavior ghost in activeGhosts)
        {
            if (ghost.state == GhostBehavior.GhostState.Scared)
                ghost.ResetScareTime();

            ghost.state = GhostBehavior.GhostState.Scared;
        }

        foreach (GakManBehavior gakMan in gakMen)
        {
            gakMan.InstantEatTouchingGhosts();
        }
    }

    private void Gizmos()
    {
        // draw nodes
        GLGizmos.SetColor(new Color(0, 1, 0, .75f));
        foreach (Vector2 nodePos in TileMapProcessor.nodePositions)
            GLGizmos.DrawSolidCircle(nodePos, .1f);

        // ghost gizmos
        foreach (GhostBehavior gst in ghosts)
            gst.Gizmos();

        // teleportal gizmos
        //foreach (TeleportReference tpr in teleportReferences)
        //    tpr.Gizmos();

        //// hasTile
        //GLGizmos.SetColor(TileMapProcessor.HasTile(Camera.main.ScreenToWorldPoint(Mouse.current.position.value), true, false) ? Color.red : Color.green);
        //GLGizmos.DrawBoxRing(Camera.main.ScreenToWorldPoint(Mouse.current.position.value + Vector2.one).Round(), Vector2.one * 1f, .2f);
    }
}
