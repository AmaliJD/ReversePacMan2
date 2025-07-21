using UnityEngine;
using UnityEngine.Tilemaps;
using GLG;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using EX;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Main : MonoBehaviour
{
    public Tilemap WallTilemap;
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

        TileMapProcessor.Init(WallTilemap);
        TileMapProcessor.SearchTilemapForNodes();
        TileMapProcessor.RemoveIgnoreTiles();

        InputProcessor.input = GetComponent<PlayerInput>();
        //StartCoroutine(LoadGhostSprites());
    }

    private IEnumerator LoadGhostSprites()
    {
        AsyncOperationHandle<Sprite[]> ghostAssetHandle = Addressables.LoadAssetAsync<Sprite[]>("Assets/Images/ghost.png");
        yield return ghostAssetHandle;

        if (ghostAssetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (Sprite sprite in ghostAssetHandle.Result)
                ghostSprites.Add(sprite);
        }
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
    }

    private void Update()
    {
        InputProcessor.GetInputs();

        if (InputProcessor.input.actions["S1"].WasPressedThisFrame())
            drawGizmos = !drawGizmos;

        if (InputProcessor.input.actions["S2"].WasPressedThisFrame())
        {
            foreach (GhostBehavior ghost in ghosts)
                ghost.state = GhostBehavior.GhostState.Eaten;
        }

        if (InputProcessor.input.actions["S3"].WasPressedThisFrame())
        {
            ScareGhosts();
        }

        if (InputProcessor.input.actions["S4"].WasPressedThisFrame())
        {
            if (!scatterMode)
                ScatterGhosts();
            else
                UnScatterGhosts();
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

        if (drawGizmos)
            Gizmos();
    }

    void ScatterGhosts()
    {
        scatterMode = true;
        scatterTargets = scatterTargets.Shuffle();
        List<GhostBehavior> activeGhosts = ghosts.Where(x => x.state == GhostBehavior.GhostState.Chase).ToList();

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

        foreach (GhostBehavior ghost in ghosts)
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
    }
}
