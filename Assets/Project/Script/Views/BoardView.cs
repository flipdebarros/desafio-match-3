using System;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Project.Script.Utils;
using Gazeus.DesafioMatch3.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    public class BoardView : MonoBehaviour
    {
        public event Action<int, int> TileClicked;
        public event Action<int> OnTilesDestroyed;

        [SerializeField] private GridLayoutGroup _boardContainer;
        [SerializeField] private TileColorRepository _tileColorRepository;
        [SerializeField] private TileView _tilePrefab;
        [SerializeField] private TileSpotView _tileSpotPrefab;
        
        private TileView[][] _tiles;
        private TileSpotView[][] _tileSpots;
        private ObjectPool<TileView> _tilePool;

        public void CreateBoard(Tile[][] board)
        {
            int width = board[0].Length;
            int height = board.Length;
            
            _boardContainer.constraintCount = width;
            _tiles = new TileView[height][];
            _tileSpots = new TileSpotView[height][];
            _tilePool = new ObjectPool<TileView>(transform, _tilePrefab);

            for (int y = 0; y < height; y++)
            {
                _tiles[y] = new TileView[width];
                _tileSpots[y] = new TileSpotView[width];

                for (int x = 0; x < width; x++)
                {
                    TileSpotView tileSpot = Instantiate(_tileSpotPrefab, _boardContainer.transform, false);
                    tileSpot.SetPosition(x, y);
                    tileSpot.Clicked += TileSpot_Clicked;

                    _tileSpots[y][x] = tileSpot;

                    int tileTypeIndex = board[y][x].Type;
                    if (tileTypeIndex > -1)
                    {
                        TileView tile = _tilePool.GetNextObject();
                        tile.Setup(_tileColorRepository.TileTypeColorList[tileTypeIndex]);
                        tileSpot.SetTile(tile.gameObject);

                        _tiles[y][x] = tile;
                    }
                }
            }
        }

        public Tween CreateTile(List<AddedTileInfo> addedTiles)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < addedTiles.Count; i++)
            {
                AddedTileInfo addedTileInfo = addedTiles[i];
                Vector2Int position = addedTileInfo.Position;

                TileSpotView tileSpot = _tileSpots[position.y][position.x];
                
                TileView tile = _tilePool.GetNextObject();
                tile.Setup(_tileColorRepository.TileTypeColorList[addedTileInfo.Type]);
                tileSpot.SetTile(tile.gameObject);

                _tiles[position.y][position.x] = tile;

                tile.transform.localScale = Vector2.zero;
                sequence.Join(tile.transform.DOScale(1.0f, 0.2f));
            }

            return sequence;
        }

        public Tween DestroyTiles(List<Vector2Int> matchedPosition)
        {
            int tileCount = matchedPosition.Count;
            for (int i = 0; i < tileCount; i++)
            {
                Vector2Int position = matchedPosition[i];
                _tilePool.ReleaseObject(_tiles[position.y][position.x]);
                _tiles[position.y][position.x] = null;
            }
            
            OnTilesDestroyed?.Invoke(tileCount);
            return DOVirtual.DelayedCall(0.2f, () => {});
        }

        public Tween MoveTiles(List<MovedTileInfo> movedTiles)
        {
            TileView[][] tiles = new TileView[_tiles.Length][];
            for (int y = 0; y < _tiles.Length; y++)
            {
                tiles[y] = new TileView[_tiles[y].Length];
                for (int x = 0; x < _tiles[y].Length; x++)
                {
                    tiles[y][x] = _tiles[y][x];
                }
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < movedTiles.Count; i++)
            {
                MovedTileInfo movedTileInfo = movedTiles[i];

                Vector2Int from = movedTileInfo.From;
                Vector2Int to = movedTileInfo.To;

                sequence.Join(_tileSpots[to.y][to.x].AnimatedSetTile(_tiles[from.y][from.x].gameObject));

                tiles[to.y][to.x] = _tiles[from.y][from.x];
            }

            _tiles = tiles;

            return sequence;
        }

        public Tween SwapTiles(int fromX, int fromY, int toX, int toY)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_tileSpots[fromY][fromX].AnimatedSetTile(_tiles[toY][toX].gameObject));
            sequence.Join(_tileSpots[toY][toX].AnimatedSetTile(_tiles[fromY][fromX].gameObject));

            (_tiles[toY][toX], _tiles[fromY][fromX]) = (_tiles[fromY][fromX], _tiles[toY][toX]);

            return sequence;
        }

        #region Events
        private void TileSpot_Clicked(int x, int y)
        {
            TileClicked(x, y);
        }
        #endregion
    }
}
