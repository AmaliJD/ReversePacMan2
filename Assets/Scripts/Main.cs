using UnityEngine;
using UnityEngine.Tilemaps;
using GLG;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using EX;

public class Main : MonoBehaviour
{
    public Tilemap WallTilemap;
    List<CharacterMovement> characters = new();

    [HideInInspector]
    public Dictionary<Vector2, Vector2> teleportReferences = new();

    private void Awake()
    {
        Static.main = this;

        TileMapProcessor.Init(WallTilemap);
        TileMapProcessor.SearchTilemapForNodes();
        TileMapProcessor.RemoveIgnoreTiles();

        InputProcessor.input = GetComponent<PlayerInput>();
    }

    public void AddCharacter(CharacterMovement character) => characters.Add(character);
    public void AddTPRef(TeleportReference tpr) => teleportReferences.Add(tpr.transform.position.Round(), tpr.target.transform.position.Round());

    private void Update()
    {
        InputProcessor.GetInputs();

        // move all characters
        foreach (CharacterMovement character in characters)
            character.Move();

        // draw nodes
        GLGizmos.SetColor(new Color(0, 1, 0, .75f));
        foreach (Vector2 nodePos in TileMapProcessor.nodePositions)
            GLGizmos.DrawSolidCircle(nodePos, .1f);
    }
}
