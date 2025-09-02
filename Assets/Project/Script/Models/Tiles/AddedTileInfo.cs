using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public struct AddedTileInfo
    {
        public Vector2Int Position { get; set; }
        public TileType Type { get; set; }
        public TileVariation Variation { get; set; }
    }
}
