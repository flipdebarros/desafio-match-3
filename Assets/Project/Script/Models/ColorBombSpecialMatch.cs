using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public class ColorBombSpecialMatch : ISpecialMatch
    {
        public bool IsConditionMet(int horizontalMatches, int verticalMatches)
        {
            return horizontalMatches >= 5 || verticalMatches >= 5;
        }
        
        public List<Vector2Int> AffectedTiles(in List<List<Tile>> board, Vector2Int position, Tile tile)
        {
            return GetAllTilesOfType(board, tile.Type);
        }
        
        private static List<Vector2Int> GetAllTilesOfType(List<List<Tile>> newBoard, int type)
        {
            List<Vector2Int> tiles = new();
            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    if (newBoard[y][x].Type != type)
                        continue;
                    tiles.Add(new Vector2Int(x, y));
                }
            }
            return tiles;
        }
    }
}
