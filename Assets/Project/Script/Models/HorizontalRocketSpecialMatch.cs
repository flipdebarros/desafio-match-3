using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public class HorizontalRocketSpecialMatch : ISpecialMatch
    {
        public bool IsConditionMet(int horizontalMatches, int verticalMatches) => 
            verticalMatches >= 4;

        public TileType GetSpecialItemType() => TileType.HorizontalRocket;
    }
}
