using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public class VerticalLineSweepSpecialMatch : ISpecialMatch
    {
        public bool IsConditionMet(int horizontalMatches, int verticalMatches)
        {
            return horizontalMatches >= 4;
        }
        
        public List<Vector2Int> AffectedTiles(in Tile[][] board, Vector2Int position, Tile tile)
        {
            List<Vector2Int> affectedTiles = new();
            for (int y = 0; y < board.Length; y++) 
                affectedTiles.Add(new Vector2Int(position.x, y));
            return affectedTiles;
        }
    }
}
