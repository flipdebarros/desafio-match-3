using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public class SimpleBombSpecialMatch : ISpecialMatch
    {
        public bool IsConditionMet(int horizontalMatches, int verticalMatches) => 
            horizontalMatches >= 3 && verticalMatches >= 3;

        public TileType GetSpecialItemType() => TileType.SimpleBomb;
    }
}
