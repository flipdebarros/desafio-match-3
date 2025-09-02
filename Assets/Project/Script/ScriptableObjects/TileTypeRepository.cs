using System;
using System.Collections.Generic;
using System.Linq;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gazeus.DesafioMatch3.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TileColorRepository", menuName = "Gameplay/TileColorRepository")]
    public class TileTypeRepository : ScriptableObject
    {
        [SerializeField] private TileTypeRepositoryEntry[] _typeEntries;
        [SerializeField] private TileVariationRepositoryEntry[] _variationEntries;

        private Dictionary<TileType, Sprite> _tileTypeDict;
        private Dictionary<TileVariation, Color> _tileVariationDict;

        public Sprite GetSpriteFromTileType(TileType type)
        {
            return _tileTypeDict?.GetValueOrDefault(type);
        }

        public Color GetColorFromTileVariation(TileVariation variation)
        {
            return _tileVariationDict?.GetValueOrDefault(variation, Color.white) ?? Color.white;
        }

        private void OnValidate()
        {
            _tileTypeDict = _typeEntries?.ToDictionary(x => x.type, x => x.sprite);
            _tileVariationDict = _variationEntries?.ToDictionary(x => x.variation, x => x.color);
        }


    }

    [Serializable]
    public struct TileVariationRepositoryEntry
    {
        public TileVariation variation;
        public Color color;
    }

    [Serializable]
    public struct TileTypeRepositoryEntry
    {
        [FormerlySerializedAs("Type")] public TileType type;
        public Sprite sprite;
    }
}
