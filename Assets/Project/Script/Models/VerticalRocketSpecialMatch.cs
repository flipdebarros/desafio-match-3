using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public class VerticalRocketSpecialMatch : ISpecialMatch
    {
        public bool IsConditionMet(int horizontalMatches, int verticalMatches) => 
            horizontalMatches >= 4;

        public TileType GetSpecialItemType() => TileType.VerticalRocket;
    }
}
