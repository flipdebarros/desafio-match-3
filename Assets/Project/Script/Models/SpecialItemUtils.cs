using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public static class SpecialItemUtils
    {
        public static List<Vector2Int> GetAffectedTiles(in Tile[][] board, TileType specialType, TileVariation variation, Vector2Int position)
        {
            switch (specialType)
            {
                case TileType.ColorBomb:
                    return GetAllTilesOfVariation(board, variation);
                case TileType.SimpleBomb:
                    return GetAllTilesInANeighbourhood(board, position, 2);
                case TileType.VerticalRocket:
                    return GetAllTilesInAVerticalLine(board, position);
                case TileType.HorizontalRocket:
                    return GetAllTilesInAHorizontalLine(board, position);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static List<Vector2Int> GetAllTilesInAHorizontalLine(Tile[][] board, Vector2Int position)
        {
            List<Vector2Int> affectedTiles = new();
            int y = position.y;
            for (int x = 0; x < board[y].Length; x++)
            {
                if (board[y][x].Type is TileType.Simple)
                    affectedTiles.Add(new Vector2Int(x, y));
            }
            return affectedTiles;
        }
        
        private static List<Vector2Int> GetAllTilesInAVerticalLine(Tile[][] board, Vector2Int position)
        {
            List<Vector2Int> affectedTiles = new();
            int x = position.x;
            for (int y = 0; y < board.Length; y++)
            {
                if(board[y][x].Type is TileType.Simple)
                    affectedTiles.Add(new Vector2Int(x, y));
            }
            return affectedTiles;
        }
        
        private static List<Vector2Int> GetAllTilesInANeighbourhood(Tile[][] board, Vector2Int position, int size)
        {
            List<Vector2Int> affectedTiles = new();
            for (int dx = -size; dx <= size; dx++)
            {
                for (int dy = -size; dy <= size; dy++)
                {
                    (int x, int y) = (position.x + dx, position.y + dy);
                    if (!IsValidCoordinates(x, y, board[0].Length, board.Length) || board[y][x].Type is not TileType.Simple)
                        continue;

                    affectedTiles.Add(new Vector2Int(position.x + dx, position.y + dy));
                }
            }
            return affectedTiles;
        }
        
        private static List<Vector2Int> GetAllTilesOfVariation(Tile[][] newBoard, TileVariation variation)
        {
            List<Vector2Int> tiles = new();
            for (int y = 0; y < newBoard.Length; y++)
            {
                for (int x = 0; x < newBoard[y].Length; x++)
                {
                    Tile tile = newBoard[y][x];
                    if (tile.Type is not TileType.Simple || tile.Variation != variation)
                        continue;
                    tiles.Add(new Vector2Int(x, y));
                }
            }
            return tiles;
        }
        
        private static bool IsValidCoordinates(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
        
    }
}
