using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Models
{
    public class BoardModel
    {
        private Tile[][] _boardTiles;
        private List<TileVariation> _tilesVariations;
        private List<ISpecialMatch> _specialMatches;
        private int _tileCount;

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            Tile toTile = _boardTiles[toY][toX];
            Tile fromTile = _boardTiles[fromY][fromX];

            TileType toType = toTile.Type;
            TileType fromType = fromTile.Type;
            
            if (toType is TileType.None || fromType is TileType.None)
                return false;
            
            if (toType is not TileType.Simple && fromType is not TileType.Simple)
                return false;
            
            if (toTile.Variation is TileVariation.None && fromTile.Variation is TileVariation.None)
                return false;

            if (toType is not TileType.Simple && fromTile.Variation is not TileVariation.None)
                return true;
                    
            if (fromType is not TileType.Simple && toTile.Variation is not TileVariation.None)
                return true;
            
            return CheckMatches(_boardTiles, fromX, fromY, toTile.Variation) || CheckMatches(_boardTiles, toX, toY, fromTile.Variation);
        }

        public Tile[][] StartGame(int boardWidth, int boardHeight)
        {
            _tilesVariations = new List<TileVariation> { TileVariation.Blue, TileVariation.Green, TileVariation.Orange, TileVariation.Yellow };
            _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesVariations);
            _specialMatches = new List<ISpecialMatch>
            {
                new ColorBombSpecialMatch(),
                new SimpleBombSpecialMatch(),
                new VerticalRocketSpecialMatch(),
                new HorizontalRocketSpecialMatch()
            };

            return _boardTiles;
        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {
            Tile[][] newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            List<BoardSequence> boardSequences = new();

            List<Vector2Int> changedTiles = new();
            changedTiles.Add(new Vector2Int(fromX, fromY));
            changedTiles.Add(new Vector2Int(toX, toY));
            
            HashSet<Vector2Int> matchedPosition = FindMatches(
                newBoard,
                changedTiles,
                out Dictionary<Vector2Int, (int horizontal, int vertical)> matchStats
            );
            
            TryActivateSpecialItems(newBoard, matchedPosition, fromX, fromY, toX, toY);
            
            while (matchedPosition.Count > 0)
            {
                changedTiles.Clear();

                List<AddedSpecialItemInfo> addedSpecialItems = CheckSpecialMatches(newBoard, matchStats, matchedPosition);

                //Cleaning the matched tiles
                foreach (Vector2Int pos in matchedPosition)
                {
                    Tile tile = newBoard[pos.y][pos.x];
                    tile.Type = TileType.None;
                    tile.Variation = TileVariation.None;
                    tile.Id = -1;
                }

                List<MovedTileInfo> movedTilesList = DropTiles(newBoard, matchedPosition, out List<Vector2Int> emptySpots);
                changedTiles.AddRange(movedTilesList.Select(x => x.To));

                List<AddedTileInfo> addedTiles = FillBoard(newBoard, emptySpots);
                changedTiles.AddRange(emptySpots);

                BoardSequence sequence = new()
                {
                    MatchedPosition = matchedPosition.ToList(),
                    MovedTiles = movedTilesList,
                    AddedTiles = addedTiles,
                    AddedSpecialItems = addedSpecialItems
                };
                boardSequences.Add(sequence);
                matchedPosition = FindMatches(newBoard, changedTiles, out matchStats);
            }

            _boardTiles = newBoard;

            return boardSequences;
        }
        
        private List<MovedTileInfo> DropTiles(Tile[][] newBoard, HashSet<Vector2Int> matchedTiles, out List<Vector2Int> emptySpots)
        {
            Dictionary<int, MovedTileInfo> movedTiles = new();
            List<MovedTileInfo> movedTilesList = new();
            Dictionary<int, int> lowestGapInColumn = GetLowestGapInColumns(matchedTiles);
            emptySpots = new List<Vector2Int>();

            foreach ((int x, int y) in lowestGapInColumn)
            {
                int emptyCount = 0;
                emptySpots.Add(new Vector2Int(x, emptyCount));

                if (y == 0) continue;

                int gap = y;
                for (int k = y - 1; k >= 0; k--)
                {
                    if (newBoard[k][x].Type is TileType.None)
                    {
                        emptySpots.Add(new Vector2Int(x, ++emptyCount));
                        continue;
                    }

                    (newBoard[gap][x], newBoard[k][x]) = (newBoard[k][x], newBoard[gap][x]);

                    MovedTileInfo movedTileInfo = new()
                    {
                        From = new Vector2Int(x, k),
                        To = new Vector2Int(x, gap)
                    };
                    movedTilesList.Add(movedTileInfo);
                    gap--;
                }
            }

            return movedTilesList;
        }

        private List<AddedTileInfo> FillBoard(Tile[][] newBoard, List<Vector2Int> emptySpots)
        {
            List<AddedTileInfo> addedTiles = new();
            foreach (Vector2Int spot in emptySpots)
            {
                (int x, int y) = (spot.x, spot.y);
                int tileVariation = Random.Range(0, _tilesVariations.Count);
                Tile tile = newBoard[y][x];
                tile.Id = _tileCount++;
                tile.Variation = _tilesVariations[tileVariation];
                tile.Type = TileType.Simple;
                addedTiles.Add(new AddedTileInfo
                {
                    Position = new Vector2Int(x, y),
                    Type = tile.Type,
                    Variation = tile.Variation
                });
            }
            return addedTiles;
        }

        private Tile[][] CreateBoard(int width, int height, List<TileVariation> tileVariations)
        {
            Tile[][] board = new Tile[height][];
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                board[y] = new Tile[width];
                for (int x = 0; x < width; x++)
                {
                    board[y][x] = new Tile { Id = -1, Type = TileType.None, Variation = TileVariation.None };
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<TileVariation> noMatchVariations = new(tileVariations.Count);
                    for (int i = 0; i < tileVariations.Count; i++)
                    {
                        noMatchVariations.Add(_tilesVariations[i]);
                    }

                    if (x > 1 &&
                        board[y][x - 1].Variation == board[y][x - 2].Variation)
                    {
                        noMatchVariations.Remove(board[y][x - 1].Variation);
                    }

                    if (y > 1 &&
                        board[y - 1][x].Variation == board[y - 2][x].Variation)
                    {
                        noMatchVariations.Remove(board[y - 1][x].Variation);
                    }

                    board[y][x].Id = _tileCount++;
                    board[y][x].Variation = noMatchVariations[Random.Range(0, noMatchVariations.Count)];
                    board[y][x].Type = TileType.Simple;
                }
            }

            return board;
        }

        private List<AddedSpecialItemInfo> CheckSpecialMatches(
            Tile[][] newBoard,
            Dictionary<Vector2Int, (int horizontal, int vertical)> matchStats,
            HashSet<Vector2Int> matchedPosition
        )
        {
            List<AddedSpecialItemInfo> addedSpecialItems = new();
            foreach ((Vector2Int pos, (int horizontal, int vertical)) in matchStats)
            {
                foreach (ISpecialMatch specialMatch in _specialMatches)
                {
                    if (!specialMatch.IsConditionMet(horizontal, vertical))
                        continue;

                    //matchedPosition.UnionWith(specialMatch.AffectedTiles(newBoard, pos, newBoard[pos.y][pos.x]));
                    matchedPosition.Remove(pos);
                    Tile tile = newBoard[pos.y][pos.x];
                    tile.Type = specialMatch.GetSpecialItemType();
                    tile.Variation = TileVariation.None;
                    addedSpecialItems.Add(new AddedSpecialItemInfo { Position = pos, Type = tile.Type, Variation = tile.Variation });
                    break;
                }
            }
            return addedSpecialItems;
        }

        private static void TryActivateSpecialItems(Tile[][] newBoard, HashSet<Vector2Int> matchedTiles, int fromX, int fromY, int toX, int toY)
        {
            Tile toTile = newBoard[toY][toX];
            Tile fromTile = newBoard[fromY][fromX];

            if (fromTile.Type is TileType.Simple && toTile.Type is TileType.Simple)
                return;

            Vector2Int position;
            if (fromTile.Type is not TileType.Simple)
            {
                position = new Vector2Int(fromX, fromY);
                matchedTiles.UnionWith(SpecialItemUtils.GetAffectedTiles(newBoard, fromTile.Type, toTile.Variation, position));
            }
            else
            {
                position = new Vector2Int(toX, toY);
                matchedTiles.UnionWith(SpecialItemUtils.GetAffectedTiles(newBoard, toTile.Type, fromTile.Variation, position));
            }
            
            matchedTiles.Add(position);
        }
        
        private static Dictionary<int, int> GetLowestGapInColumns(HashSet<Vector2Int> removedTiles)
        {
            Dictionary<int, int> lowestGapInColumn = new();
            foreach (Vector2Int tile in removedTiles)
            {
                (int x, int y) = (tile.x, tile.y);
                if (!lowestGapInColumn.TryAdd(x, y))
                    lowestGapInColumn[x] = y > lowestGapInColumn[x] ? y : lowestGapInColumn[x];
            }
            return lowestGapInColumn;
        }

        private static Tile[][] CopyBoard(Tile[][] boardToCopy)
        {
            Tile[][] newBoard = new Tile[boardToCopy.Length][];
            for (int y = 0; y < boardToCopy.Length; y++)
            {
                newBoard[y] = new Tile[boardToCopy[y].Length];
                for (int x = 0; x < boardToCopy[y].Length; x++)
                {
                    Tile tile = boardToCopy[y][x];
                    newBoard[y][x] = new Tile { Id = tile.Id, Type = tile.Type, Variation = tile.Variation };
                }
            }

            return newBoard;
        }

        private static HashSet<Vector2Int> FindMatches(
            Tile[][] newBoard,
            List<Vector2Int> changedTiles,
            out Dictionary<Vector2Int, (int horizontal, int vertical)> matchStats
        )
        {
            matchStats = new Dictionary<Vector2Int, (int horizontal, int vertical)>();

            HashSet<Vector2Int> matchedTiles = new();
            HashSet<Vector2Int> visitedTiles = new();
            Stack<Vector2Int> tileStack = new();

            int width = newBoard[0].Length;
            int height = newBoard.Length;

            foreach (Vector2Int pos in changedTiles)
            {
                if (visitedTiles.Contains(pos))
                    continue;

                TileVariation variation = newBoard[pos.y][pos.x].Variation;
                tileStack.Push(pos);

                bool matched = false;
                int maxHorizontal = 0;
                int maxVertical = 0;

                while (tileStack.Count > 0)
                {
                    Vector2Int curr = tileStack.Pop();
                    (int x, int y) = (curr.x, curr.y);

                    if (!IsValidCoordinates(x, y, width, height))
                        continue;

                    Tile tile = newBoard[y][x];
                    if (tile.Type is not TileType.Simple || tile.Variation != variation || !visitedTiles.Add(curr))
                        continue;

                    int horizontalMatches = CheckMatchHorizontal(newBoard, x, y, variation);
                    int verticalMatches = CheckMatchVertical(newBoard, x, y, variation);

                    if (horizontalMatches < 3 && verticalMatches < 3)
                        continue;

                    matchedTiles.Add(new Vector2Int(x, y));
                    matched = true;
                    maxHorizontal = horizontalMatches > maxHorizontal ? horizontalMatches : maxHorizontal;
                    maxVertical = verticalMatches > maxVertical ? verticalMatches : maxVertical;

                    tileStack.Push(new Vector2Int(x - 1, y));
                    tileStack.Push(new Vector2Int(x + 1, y));
                    tileStack.Push(new Vector2Int(x, y - 1));
                    tileStack.Push(new Vector2Int(x, y + 1));
                }

                if (!matched) continue;
                matchStats.Add(pos, (maxHorizontal, maxVertical));
            }

            return matchedTiles;
        }

        private static bool CheckMatches(Tile[][] newBoard, int x, int y, TileVariation variation) => 
            CheckMatchHorizontal(newBoard, x, y, variation) >= 3 || CheckMatchVertical(newBoard, x, y, variation) >= 3;

        private static int CheckMatchHorizontal(Tile[][] board, int x, int y, TileVariation variation)
        {
            if (variation is TileVariation.None)
                return 0;
            
            int count = 1;
            if (x > 1 && variation == board[y][x - 1].Variation && variation == board[y][x - 2].Variation)
                count += 2;
            else if (x > 0 && variation == board[y][x - 1].Variation)
                count++;

            if (x < board[y].Length - 2 && variation == board[y][x + 1].Variation && variation == board[y][x + 2].Variation)
                count += 2;
            else if (x < board[y].Length - 1 && variation == board[y][x + 1].Variation)
                count++;

            return count;
        }

        private static int CheckMatchVertical(Tile[][] board, int x, int y, TileVariation variation)
        {
            if (variation is TileVariation.None)
                return 0;
            
            int count = 1;
            if (y > 1 && variation == board[y - 1][x].Variation && variation == board[y - 2][x].Variation)
                count += 2;
            else if (y > 0 && variation == board[y - 1][x].Variation)
                count++;

            if (y < board.Length - 2 && variation == board[y + 1][x].Variation && variation == board[y + 2][x].Variation)
                count += 2;
            else if (y < board.Length - 1 && variation == board[y + 1][x].Variation)
                count++;

            return count;
        }

        private static bool IsValidCoordinates(int x, int y, int width, int height) => x >= 0 && x < width && y >= 0 && y < height;
    }
}
