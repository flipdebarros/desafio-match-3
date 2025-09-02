using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Models
{
    public class BoardModel
    {
        private Tile[][] _boardTiles;
        private List<int> _tilesTypes;
        private List<ISpecialMatch> _specialMatches;
        private int _tileCount;

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY) =>
            CheckMatches(_boardTiles, fromX, fromY, _boardTiles[toY][toX].Type) || CheckMatches(_boardTiles, toX, toY, _boardTiles[fromY][fromX].Type);

        public Tile[][] StartGame(int boardWidth, int boardHeight)
        {
            _tilesTypes = new List<int> { 0, 1, 2, 3 };
            _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);
            _specialMatches = new List<ISpecialMatch>
            {
                new ColorBombSpecialMatch(),
                new BombSpecialMatch(2),
                new VerticalLineSweepSpecialMatch(),
                new HorizontalLineSweepSpecialMatch()
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

            while (matchedPosition.Count > 0)
            {
                changedTiles.Clear();

                CheckSpecialMatches(newBoard, matchStats, matchedPosition);

                //Cleaning the matched tiles
                foreach (Vector2Int pos in matchedPosition)
                {
                    Tile tile = newBoard[pos.y][pos.x];
                    tile.Type = -1;
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
                    if (newBoard[k][x].Type == -1)
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
                int tileType = Random.Range(0, _tilesTypes.Count);
                Tile tile = newBoard[y][x];
                tile.Id = _tileCount++;
                tile.Type = _tilesTypes[tileType];
                addedTiles.Add(new AddedTileInfo
                {
                    Position = new Vector2Int(x, y),
                    Type = tile.Type
                });
            }
            return addedTiles;
        }
        
        private Tile[][] CreateBoard(int width, int height, List<int> tileTypes)
        {
            Tile[][] board = new Tile[height][];
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                board[y] = new Tile[width];
                for (int x = 0; x < width; x++)
                {
                    board[y][x] = new Tile { Id = -1, Type = -1 };
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<int> noMatchTypes = new(tileTypes.Count);
                    for (int i = 0; i < tileTypes.Count; i++)
                    {
                        noMatchTypes.Add(_tilesTypes[i]);
                    }

                    if (x > 1 &&
                        board[y][x - 1].Type == board[y][x - 2].Type)
                    {
                        noMatchTypes.Remove(board[y][x - 1].Type);
                    }

                    if (y > 1 &&
                        board[y - 1][x].Type == board[y - 2][x].Type)
                    {
                        noMatchTypes.Remove(board[y - 1][x].Type);
                    }

                    board[y][x].Id = _tileCount++;
                    board[y][x].Type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
                }
            }

            return board;
        }
        
        private void CheckSpecialMatches(
            Tile[][] newBoard,
            Dictionary<Vector2Int, (int horizontal, int vertical)> matchStats,
            HashSet<Vector2Int> matchedPosition
        )
        {
            foreach ((Vector2Int pos, (int horizontal, int vertical)) in matchStats)
            {
                foreach (ISpecialMatch specialMatch in _specialMatches)
                {
                    if (!specialMatch.IsConditionMet(horizontal, vertical))
                        continue;

                    matchedPosition.UnionWith(specialMatch.AffectedTiles(newBoard, pos, newBoard[pos.y][pos.x]));
                    break;
                }
            }
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
                    newBoard[y][x] = new Tile { Id = tile.Id, Type = tile.Type };
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

            foreach (Vector2Int tile in changedTiles)
            {
                if (visitedTiles.Contains(tile))
                    continue;

                int type = newBoard[tile.y][tile.x].Type;
                tileStack.Push(tile);

                bool matched = false;
                int maxHorizontal = 0;
                int maxVertical = 0;

                while (tileStack.Count > 0)
                {
                    Vector2Int curr = tileStack.Pop();
                    (int x, int y) = (curr.x, curr.y);

                    if (!IsValidCoordinates(x, y, width, height) || newBoard[y][x].Type != type || !visitedTiles.Add(curr))
                        continue;

                    int horizontalMatches = CheckMatchHorizontal(newBoard, x, y, type);
                    int verticalMatches = CheckMatchVertical(newBoard, x, y, type);

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
                matchStats.Add(tile, (maxHorizontal, maxVertical));
            }

            return matchedTiles;
        }

        private static bool CheckMatches(Tile[][] newBoard, int x, int y, int type) =>
            CheckMatchHorizontal(newBoard, x, y, type) >= 3 || CheckMatchVertical(newBoard, x, y, type) >= 3;

        private static int CheckMatchHorizontal(Tile[][] board, int x, int y, int type)
        {
            int count = 1;
            if (x > 1 && type == board[y][x - 1].Type && type == board[y][x - 2].Type)
                count += 2;
            else if (x > 0 && type == board[y][x - 1].Type)
                count++;

            if (x < board[y].Length - 2 && type == board[y][x + 1].Type && type == board[y][x + 2].Type)
                count += 2;
            else if (x < board[y].Length - 1 && type == board[y][x + 1].Type)
                count++;

            return count;
        }

        private static int CheckMatchVertical(Tile[][] board, int x, int y, int type)
        {
            int count = 1;
            if (y > 1 && type == board[y - 1][x].Type && type == board[y - 2][x].Type)
                count += 2;
            else if (y > 0 && type == board[y - 1][x].Type)
                count++;

            if (y < board.Length - 2 && type == board[y + 1][x].Type && type == board[y + 2][x].Type)
                count += 2;
            else if (y < board.Length - 1 && type == board[y + 1][x].Type)
                count++;

            return count;
        }

        private static bool IsValidCoordinates(int x, int y, int width, int height) => x >= 0 && x < width && y >= 0 && y < height;
    }
}
