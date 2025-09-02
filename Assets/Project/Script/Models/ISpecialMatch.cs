using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public interface ISpecialMatch
    {
        public bool IsConditionMet(int horizontalMatches, int verticalMatches);
        public List<Vector2Int> AffectedTiles(in Tile[][] board, Vector2Int position, Tile tile);
    }

}
