using UnityEngine;
using UnityEngine.Tilemaps;
using GLG;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    public Tilemap WallTilemap;
    List<CharacterMovement> characters = new();

    private void Awake()
    {
        Static.main = this;

        TileMapProcessor.Init(WallTilemap);
        TileMapProcessor.SearchTilemapForNodes();
        TileMapProcessor.RemoveIgnoreTiles();

        InputProcessor.input = GetComponent<PlayerInput>();
    }

    public void AddCharacter(CharacterMovement character) => characters.Add(character);

    private void Update()
    {
        InputProcessor.GetInputs();

        foreach (CharacterMovement character in characters)
            character.Move();

        GLGizmos.SetColor(new Color(0, 1, 0, .75f));
        foreach (Vector2 nodePos in TileMapProcessor.positions)
            GLGizmos.DrawSolidCircle(nodePos, .1f);
    }
}
