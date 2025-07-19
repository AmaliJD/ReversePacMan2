using UnityEngine;
using UnityEngine.Tilemaps;
using GLG;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using EX;
using UnityEngine.Windows;

public class Main : MonoBehaviour
{
    public Tilemap WallTilemap;
    List<MovementController> movementCtrlrs = new();
    List<GhostBehavior> ghosts = new();

    [HideInInspector]
    public Dictionary<Vector2, Vector2> teleportalPairs = new();
    List<TeleportReference> teleportReferences = new();

    [HideInInspector]
    public List<HomeCell> homeCells = new();
    [HideInInspector]
    public List<HomeCell> homeCellEntrances = new();

    [HideInInspector]
    public List<GakManBehavior> gakMen = new();

    [Min(0)]
    public float gameSpeed = 1;

    public bool drawGizmos = true;

    private void Awake()
    {
        Static.main = this;

        TileMapProcessor.Init(WallTilemap);
        TileMapProcessor.SearchTilemapForNodes();
        TileMapProcessor.RemoveIgnoreTiles();

        InputProcessor.input = GetComponent<PlayerInput>();
    }

    public void AddMovementController(MovementController character) => movementCtrlrs.Add(character);
    public void AddGhost(GhostBehavior ghost) => ghosts.Add(ghost);
    public void AddGakMen(GakManBehavior gakMan) => gakMen.Add(gakMan);
    public void AddTPRef(TeleportReference tpr)
    {
        teleportalPairs.Add(tpr.transform.position.Round(), tpr.target.transform.position.Round());
        teleportReferences.Add(tpr);
    }
    public void AddHomeCell(HomeCell hc, bool isEntrance)
    {
        homeCells.Add(hc);

        if (isEntrance)
            homeCellEntrances.Add(hc);
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

        // update ghost behavior
        foreach (GhostBehavior ghost in ghosts)
            ghost.BehaviorUpdate();

        // move all characters
        foreach (MovementController character in movementCtrlrs)
            character.Move();

        if (drawGizmos)
            Gizmos();
    }

    private void Gizmos()
    {
        // draw nodes
        GLGizmos.SetColor(new Color(0, 1, 0, .75f));
        foreach (Vector2 nodePos in TileMapProcessor.nodePositions)
            GLGizmos.DrawSolidCircle(nodePos, .1f);

        // teleportal gizmos
        foreach (TeleportReference tpr in teleportReferences)
            tpr.Gizmos();
    }
}
