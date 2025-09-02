using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public class HorizontalLineSweepSpecialMatch : ISpecialMatch
    {
        public bool IsConditionMet(int horizontalMatches, int verticalMatches)
        {
            return verticalMatches >= 4;
        }
        
        public List<Vector2Int> AffectedTiles(in Tile[][] board, Vector2Int position, Tile tile)
        {
            List<Vector2Int> affectedTiles = new();
            for (int x = 0; x < board[position.y].Length; x++) 
                affectedTiles.Add(new Vector2Int(x, position.y));
            return affectedTiles;
        }
    }
}
