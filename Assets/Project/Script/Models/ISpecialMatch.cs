using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public interface ISpecialMatch
    {
        bool IsConditionMet(int horizontalMatches, int verticalMatches);
        TileType GetSpecialItemType();
    }

}
