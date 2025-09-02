using UnityEngine;

namespace Gazeus.DesafioMatch3.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TileColorRepository", menuName = "Gameplay/TileColorRepository")]
    public class TileColorRepository : ScriptableObject
    {
        [SerializeField] private Color[] _tileTypeColorList;

        public Color[] TileTypeColorList => _tileTypeColorList;
    }
}
