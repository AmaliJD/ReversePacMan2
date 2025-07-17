using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using EX;
using UnityEngine.UIElements;
public static class TileMapProcessor
{
    static Tilemap _wallTileMap;

    public static List<Vector2> positions;
    public static List<Vector2> ignoreWallPositions;

    public static void Init(Tilemap wall)
    {
        positions = new();
        ignoreWallPositions = new();

        _wallTileMap = wall;
    }

    public static void SearchTilemapForNodes()
    {
        int count = 0;
        for (int i = _wallTileMap.origin.x; i < _wallTileMap.origin.x + _wallTileMap.size.x; i++)
        {
            for (int j = _wallTileMap.origin.y; j < _wallTileMap.origin.y + _wallTileMap.size.y; j++)
            {
                if (IsTileNode(new Vector3Int(i, j)))
                    positions.Add((Vector2)_wallTileMap.CellToWorld(new Vector3Int(i, j)) + (Vector2.one * .5f));

                if (_wallTileMap.HasTile(new Vector3Int(i, j)) && _wallTileMap.GetTile(new Vector3Int(i, j)).name == "wall tiles_40")
                    ignoreWallPositions.Add(new Vector2(i, j));

                count++;
            }
        }
        Debug.Log($"Tiles Searched: {count}");
    }

    public static void RemoveIgnoreTiles()
    {
        foreach (Vector2 pos in ignoreWallPositions)
            _wallTileMap.SetTile(new Vector3Int((int)pos.x, (int)pos.y), null);
    }

    static bool IsTileNode(Vector3Int position)
    {
        if (_wallTileMap.HasTile(position))
            return false;

        bool upEmpty, rightEmpty, downEmpty, leftEmpty;
        upEmpty = !_wallTileMap.HasTile(position + Vector3Int.up);
        rightEmpty = !_wallTileMap.HasTile(position + Vector3Int.right);
        downEmpty = !_wallTileMap.HasTile(position - Vector3Int.up);
        leftEmpty = !_wallTileMap.HasTile(position - Vector3Int.right);

        bool upRightFilled, upLeftFilled, downRightFilled, downLeftFilled;
        upRightFilled = _wallTileMap.HasTile(position + Vector3Int.up + Vector3Int.right);
        upLeftFilled = _wallTileMap.HasTile(position + Vector3Int.up - Vector3Int.right);
        downRightFilled = _wallTileMap.HasTile(position - Vector3Int.up + Vector3Int.right);
        downLeftFilled = _wallTileMap.HasTile(position - Vector3Int.up - Vector3Int.right);

        return (upEmpty || downEmpty) && (rightEmpty || leftEmpty); //&& Ex.IfAtLeastX(3, upRightFilled, upLeftFilled, downRightFilled, downLeftFilled); //&& !(upEmpty && downEmpty && rightEmpty && leftEmpty);
    }

    public static bool HasTile(Vector2 position, bool countIgnoreWalls)
    {
        return _wallTileMap.HasTile((Vector3Int)(position - Vector2.one).RoundToInt()) || (countIgnoreWalls ? ignoreWallPositions.Contains(position - Vector2.one) : false);
    }
}
