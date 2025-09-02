using System;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class BoardController
    {
        public event Action<int> OnMatch 
        {
            add => _view.OnTilesDestroyed += value;
            remove => _view.OnTilesDestroyed -= value;
        }

        private readonly BoardModel _model;
        private readonly BoardView _view;
        private readonly int _boardWidth;
        private readonly int _boardHeight;

        private bool _isAnimating;
        private int _selectedX = -1;
        private int _selectedY = -1;

        public BoardController(
            BoardModel model,
            BoardView view,
            int boardWidth,
            int boardHeight
        )
        {
            _model = model;
            _view = view;
            _boardWidth = boardWidth;
            _boardHeight = boardHeight;
        }

        public void Initialize()
        {
            AddListeners();

            Tile[][] board = _model.StartGame(_boardWidth, _boardHeight);
            _view.CreateBoard(board);
        }

        public void Dispose()
        {
            RemoveListeners();
        }

        private void AddListeners()
        {
            _view.TileClicked += OnTileClick;
        }

        private void RemoveListeners()
        {
            _view.TileClicked -= OnTileClick;
        }

        private void AnimateBoard(List<BoardSequence> boardSequences, int index, Action onComplete)
        {
            BoardSequence boardSequence = boardSequences[index];

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_view.DestroyTiles(boardSequence.MatchedPosition));
            sequence.Append(_view.PlaceSpecialItems(boardSequence.AddedSpecialItems));
            sequence.Append(_view.MoveTiles(boardSequence.MovedTiles));
            sequence.Append(_view.CreateTile(boardSequence.AddedTiles));

            index += 1;
            if (index < boardSequences.Count)
            {
                sequence.onComplete += () => AnimateBoard(boardSequences, index, onComplete);
            }
            else
            {
                sequence.onComplete += () => onComplete();
            }
        }

        private void OnTileClick(int x, int y)
        {
            if (_isAnimating) return;

            if (_selectedX > -1 && _selectedY > -1)
            {
                if (Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) > 1)
                {
                    _selectedX = -1;
                    _selectedY = -1;
                }
                else
                {
                    _isAnimating = true;
                    _view.SwapTiles(_selectedX, _selectedY, x, y).onComplete += () => {
                        bool isValid = _model.IsValidMovement(_selectedX, _selectedY, x, y);
                        if (isValid)
                        {
                            List<BoardSequence> swapResult = _model.SwapTile(_selectedX, _selectedY, x, y);
                            AnimateBoard(swapResult, 0, () => _isAnimating = false);
                        }
                        else
                        {
                            _view.SwapTiles(x, y, _selectedX, _selectedY).onComplete += () => _isAnimating = false;
                        }
                        _selectedX = -1;
                        _selectedY = -1;
                    };
                }
            }
            else
            {
                _selectedX = x;
                _selectedY = y;
            }
        }
    }
}
