using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using EX;
using UnityEngine.UIElements;
public static class TileMapProcessor
{
    public static Tilemap _wallTileMap;
    public static Tilemap _secretWallTileMap;

    public static List<Vector2> nodePositions;
    public static List<Vector2> ignoreWallPositions;
    public static List<Vector2> tilePositions;

    static string ignoreTileName = "wall tiles_40";

    public static void Init(Tilemap wall, Tilemap secretWall)
    {
        nodePositions = new();
        ignoreWallPositions = new();
        tilePositions = new();

        _wallTileMap = wall;
        _secretWallTileMap = secretWall;
    }

    public static void SearchTilemapForNodes()
    {
        int count = 0;
        _wallTileMap.CompressBounds();
        _secretWallTileMap.CompressBounds();
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
        {
            if (!_secretWallTileMap.HasTile(position))
                return false;

            bool upEmptySW, rightEmptySW, downEmptySW, leftEmptySW;
            upEmptySW = _secretWallTileMap.HasTile(position + Vector3Int.up) || !_wallTileMap.HasTile(position + Vector3Int.up);
            rightEmptySW = _secretWallTileMap.HasTile(position + Vector3Int.right) || !_wallTileMap.HasTile(position + Vector3Int.right);
            downEmptySW = _secretWallTileMap.HasTile(position - Vector3Int.up) || !_wallTileMap.HasTile(position - Vector3Int.up);
            leftEmptySW = _secretWallTileMap.HasTile(position - Vector3Int.right) || !_wallTileMap.HasTile(position - Vector3Int.right);

            return ((upEmptySW || downEmptySW) && (rightEmptySW || leftEmptySW)) || Ex.IfXTrue(1, upEmptySW, downEmptySW, rightEmptySW, leftEmptySW);
        }

        bool upEmpty, rightEmpty, downEmpty, leftEmpty;
        upEmpty = !_wallTileMap.HasTile(position + Vector3Int.up) || _wallTileMap.GetTile(position + Vector3Int.up).name == ignoreTileName || _secretWallTileMap.HasTile(position + Vector3Int.up);
        rightEmpty = !_wallTileMap.HasTile(position + Vector3Int.right) || _wallTileMap.GetTile(position + Vector3Int.right).name == ignoreTileName || _secretWallTileMap.HasTile(position + Vector3Int.right);
        downEmpty = !_wallTileMap.HasTile(position - Vector3Int.up) || _wallTileMap.GetTile(position - Vector3Int.up).name == ignoreTileName || _secretWallTileMap.HasTile(position - Vector3Int.up);
        leftEmpty = !_wallTileMap.HasTile(position - Vector3Int.right) || _wallTileMap.GetTile(position - Vector3Int.right).name == ignoreTileName || _secretWallTileMap.HasTile(position - Vector3Int.right);

        return ((upEmpty || downEmpty) && (rightEmpty || leftEmpty)) || Ex.IfXTrue(1, upEmpty, downEmpty, rightEmpty, leftEmpty);
    }

    public static bool HasTile(Vector2 position, bool countIgnoreWalls, bool countSecretWalls)
    {
        bool wallHasTile = _wallTileMap.HasTile((Vector3Int)(position - Vector2.one).RoundToInt());
        bool secretWallHasTile = _secretWallTileMap.HasTile((Vector3Int)(position - Vector2.one).RoundToInt());
        bool wallHasIgnoreTile = ignoreWallPositions.Contains(position - Vector2.one);
        return (countSecretWalls ? true : !secretWallHasTile) && (wallHasTile || (countIgnoreWalls ? wallHasIgnoreTile : false));

        //return countSecretWalls switch
        //{
        //    true => wallHasTile || (countIgnoreWalls ? wallHasIgnoreTile : false),
        //    false => !secretWallHasTile && (wallHasTile || (countIgnoreWalls ? wallHasIgnoreTile : false)),
        //};

        //return _wallTileMap.HasTile((Vector3Int)(position - Vector2.one).RoundToInt()) && (!countSecretWalls ? !_secretWallTileMap.HasTile((Vector3Int)(position - Vector2.one).RoundToInt()) : true) || (countIgnoreWalls ? ignoreWallPositions.Contains(position - Vector2.one) : false);
    }
}
