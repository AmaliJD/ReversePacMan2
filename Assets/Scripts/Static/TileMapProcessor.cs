using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using EX;
using UnityEngine.UIElements;
public static class TileMapProcessor
{
    public static Tilemap _wallTileMap;

    public static List<Vector2> nodePositions;
    public static List<Vector2> ignoreWallPositions;
    public static List<Vector2> tilePositions;

    static string ignoreTileName = "wall tiles_40";

    public static void Init(Tilemap wall)
    {
        nodePositions = new();
        ignoreWallPositions = new();
        tilePositions = new();

        _wallTileMap = wall;
    }

    public static void SearchTilemapForNodes()
    {
        int count = 0;
        _wallTileMap.CompressBounds();
        for (int i = _wallTileMap.cellBounds.min.x; i < _wallTileMap.cellBounds.max.x; i++)
        {
            for (int j = _wallTileMap.cellBounds.min.y; j < _wallTileMap.cellBounds.max.y; j++)
            {
                if (IsTileNode(new Vector3Int(i, j)))
                    nodePositions.Add((Vector2)_wallTileMap.CellToWorld(new Vector3Int(i, j)) + (Vector2.one * .5f));

                if (_wallTileMap.HasTile(new Vector3Int(i, j)) && _wallTileMap.GetTile(new Vector3Int(i, j)).name == ignoreTileName)
                    ignoreWallPositions.Add(new Vector2(i, j));

                count++;
                //tilePositions.Add((Vector2)_wallTileMap.CellToWorld(new Vector3Int(i, j)) + (Vector2.one * .5f));
            }
        }
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
        upEmpty = !_wallTileMap.HasTile(position + Vector3Int.up) || _wallTileMap.GetTile(position + Vector3Int.up).name == ignoreTileName;
        rightEmpty = !_wallTileMap.HasTile(position + Vector3Int.right) || _wallTileMap.GetTile(position + Vector3Int.right).name == ignoreTileName;
        downEmpty = !_wallTileMap.HasTile(position - Vector3Int.up) || _wallTileMap.GetTile(position - Vector3Int.up).name == ignoreTileName;
        leftEmpty = !_wallTileMap.HasTile(position - Vector3Int.right) || _wallTileMap.GetTile(position - Vector3Int.right).name == ignoreTileName;

        //bool upRightFilled, upLeftFilled, downRightFilled, downLeftFilled;
        //upRightFilled = _wallTileMap.HasTile(position + Vector3Int.up + Vector3Int.right);
        //upLeftFilled = _wallTileMap.HasTile(position + Vector3Int.up - Vector3Int.right);
        //downRightFilled = _wallTileMap.HasTile(position - Vector3Int.up + Vector3Int.right);
        //downLeftFilled = _wallTileMap.HasTile(position - Vector3Int.up - Vector3Int.right);

        return ((upEmpty || downEmpty) && (rightEmpty || leftEmpty)) || Ex.IfXTrue(1, upEmpty, downEmpty, rightEmpty, leftEmpty); //&& Ex.IfAtLeastX(3, upRightFilled, upLeftFilled, downRightFilled, downLeftFilled); //&& !(upEmpty && downEmpty && rightEmpty && leftEmpty);
    }

    public static bool HasTile(Vector2 position, bool countIgnoreWalls)
    {
        return _wallTileMap.HasTile((Vector3Int)(position - Vector2.one).RoundToInt()) || (countIgnoreWalls ? ignoreWallPositions.Contains(position - Vector2.one) : false);
    }
}
