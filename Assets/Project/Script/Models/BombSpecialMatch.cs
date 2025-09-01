using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public class BombSpecialMatch : ISpecialMatch
    {
        private readonly int _size;
        
        public BombSpecialMatch(int size)
        {
            _size = size;
        }
        
        public bool IsConditionMet(int horizontalMatches, int verticalMatches)
        {
            return horizontalMatches >= 3 && verticalMatches >= 3;
        }

        public List<Vector2Int> AffectedTiles(in List<List<Tile>> board, Vector2Int position, Tile tile)
        {
            List<Vector2Int> affectedTiles = new();
            for (int dx = -_size; dx < _size; dx++)
            {
                for (int dy = -_size; dy < _size; dy++)
                {
                    if (!IsValidCoordinates(position.x + dx, position.y + dy, board[0].Count, board.Count))
                        continue;

                    affectedTiles.Add(new Vector2Int(position.x + dx, position.y + dy));
                }
            }
            return affectedTiles;
        }
        
        private static bool IsValidCoordinates(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
    }
}
