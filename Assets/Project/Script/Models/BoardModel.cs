using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Models
{
    public class BoardModel
    {
        private List<List<Tile>> _boardTiles;
        private List<int> _tilesTypes;
        private int _tileCount;

        public void Initialize() { }

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    if (x > 1 &&
                        newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                        newBoard[y][x - 1].Type == newBoard[y][x - 2].Type)
                    {
                        return true;
                    }

                    if (y > 1 &&
                        newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                        newBoard[y - 1][x].Type == newBoard[y - 2][x].Type)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
        {
            _tilesTypes = new List<int> { 0, 1, 2, 3 };
            _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);

            return _boardTiles;
        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            List<BoardSequence> boardSequences = new();
            List<Vector2Int> changedTiles = new();
            changedTiles.Add(new Vector2Int(fromX, fromY));
            changedTiles.Add(new Vector2Int(toX, toY));
            List<Vector2Int> matchedPosition = FindMatches(newBoard, changedTiles);

            while (matchedPosition.Count > 0)
            {
                changedTiles.Clear();
                
                //Cleaning the matched tiles
                foreach (Vector2Int pos in matchedPosition)
                    newBoard[pos.y][pos.x] = new Tile { Id = -1, Type = -1 };

                //The code below needs this collection to be ordered to work correctly, this is not ideal
                //Ordering only the matched positions is still better than checking every position in the matrix and adding to this list
                matchedPosition = matchedPosition.OrderBy(pos => pos.y * newBoard.Count + pos.x).ToList();

                // Dropping the tiles
                Dictionary<int, MovedTileInfo> movedTiles = new();
                List<MovedTileInfo> movedTilesList = new();
                for (int i = 0; i < matchedPosition.Count; i++)
                {
                    int x = matchedPosition[i].x;
                    int y = matchedPosition[i].y;
                    if (y > 0)
                    {
                        for (int j = y; j > 0; j--)
                        {
                            Tile movedTile = newBoard[j - 1][x];
                            newBoard[j][x] = movedTile;
                            if (movedTile.Type > -1)
                            {
                                if (movedTiles.ContainsKey(movedTile.Id))
                                {
                                    movedTiles[movedTile.Id].To = new Vector2Int(x, j);
                                }
                                else
                                {
                                    MovedTileInfo movedTileInfo = new()
                                    {
                                        From = new Vector2Int(x, j - 1),
                                        To = new Vector2Int(x, j)
                                    };
                                    movedTiles.Add(movedTile.Id, movedTileInfo);
                                    movedTilesList.Add(movedTileInfo);
                                }
                            }
                        }

                        newBoard[0][x] = new Tile
                        {
                            Id = -1,
                            Type = -1
                        };
                    }
                }

                changedTiles.AddRange(movedTilesList.Select(x => x.To));

                // Filling the board
                List<AddedTileInfo> addedTiles = new();
                for (int y = newBoard.Count - 1; y > -1; y--)
                {
                    for (int x = newBoard[y].Count - 1; x > -1; x--)
                    {
                        if (newBoard[y][x].Type == -1)
                        {
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
                    }
                }
                
                changedTiles.AddRange(addedTiles.Select(x => x.Position));

                BoardSequence sequence = new()
                {
                    MatchedPosition = matchedPosition,
                    MovedTiles = movedTilesList,
                    AddedTiles = addedTiles,
                };
                boardSequences.Add(sequence);
                matchedPosition = FindMatches(newBoard, changedTiles);
            }

            _boardTiles = newBoard;

            return boardSequences;
        }

        private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
        {
            List<List<Tile>> newBoard = new(boardToCopy.Count);
            for (int y = 0; y < boardToCopy.Count; y++)
            {
                newBoard.Add(new List<Tile>(boardToCopy[y].Count));
                for (int x = 0; x < boardToCopy[y].Count; x++)
                {
                    Tile tile = boardToCopy[y][x];
                    newBoard[y].Add(new Tile { Id = tile.Id, Type = tile.Type });
                }
            }

            return newBoard;
        }

        private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
        {
            List<List<Tile>> board = new(height);
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                board.Add(new List<Tile>(width));
                for (int x = 0; x < width; x++)
                {
                    board[y].Add(new Tile { Id = -1, Type = -1 });
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

        private static List<Vector2Int> FindMatches(List<List<Tile>> newBoard, List<Vector2Int> changedTiles)
        {
            HashSet<Vector2Int> matchedTiles = new();
            HashSet<Vector2Int> visitedTiles = new();
            Stack<Vector2Int> tileStack = new();

            int widht = newBoard[0].Count;
            int height = newBoard.Count;

            foreach (Vector2Int tile in changedTiles)
            {
                if (visitedTiles.Contains(tile))
                    continue;

                int type = newBoard[tile.y][tile.x].Type;
                tileStack.Push(tile);
                while (tileStack.Count > 0)
                {
                    Vector2Int curr = tileStack.Pop();
                    (int x, int y) = (curr.x, curr.y);

                    if (!IsValidCoordinates(x, y, widht, height) || newBoard[y][x].Type != type || !visitedTiles.Add(curr))
                        continue;

                    if(!CheckMatches(newBoard, matchedTiles, x, y))
                        continue;

                    tileStack.Push(new Vector2Int(x - 1, y));
                    tileStack.Push(new Vector2Int(x + 1, y));
                    tileStack.Push(new Vector2Int(x, y - 1));
                    tileStack.Push(new Vector2Int(x, y + 1));
                }
            }

            return matchedTiles.ToList();
        }

        private static bool CheckMatches(List<List<Tile>> newBoard, HashSet<Vector2Int> matchedTiles, int x, int y)
        {
            int horizontalMatches = CheckMatchHorizontal(newBoard, x, y);
            int verticalMatches = CheckMatchVertical(newBoard, x, y);
            
            if (horizontalMatches < 3 && verticalMatches < 3) 
                return false;
            
            matchedTiles.Add(new Vector2Int(x, y));
            return true;
        }

        private static int CheckMatchHorizontal(List<List<Tile>> board, int x, int y)
        {
            int count = 1;
            if (x > 1 && board[y][x].Type == board[y][x - 1].Type && board[y][x - 1].Type == board[y][x - 2].Type)
                count += 2;
            else if (x > 0 && board[y][x].Type == board[y][x - 1].Type)
                count++;
            
            if (x < board[y].Count - 2 && board[y][x].Type == board[y][x + 1].Type && board[y][x + 1].Type == board[y][x + 2].Type)
                count += 2;
            else if (x < board[y].Count - 1 && board[y][x].Type == board[y][x + 1].Type)
                count++;

            return count;
        }

        private static int CheckMatchVertical(List<List<Tile>> board, int x, int y)
        {
            int count = 1;
            if (y > 1 && board[y][x].Type == board[y - 1][x].Type && board[y - 1][x].Type == board[y - 2][x].Type)
                count += 2;
            else if (y > 0 && board[y][x].Type == board[y - 1][x].Type)
                count++;
            
            if (y < board.Count - 2 && board[y][x].Type == board[y + 1][x].Type && board[y + 1][x].Type == board[y + 2][x].Type)
                count += 2;
            else if (y < board.Count - 1 && board[y][x].Type == board[y + 1][x].Type)
                count++;

            return count;
        }

        private static bool IsValidCoordinates(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
    }
}
