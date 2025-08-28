using System;
using System.Collections.Generic;

namespace Gazeus.DesafioMatch3.Models
{
    public class ScoreModel
    {
        public event Action OnScoreChanged;

        public int Score {
            get => _score;
            private set {
                int previous = _score;
                _score = value;
                if (previous != value) OnScoreChanged?.Invoke();
            }
        }

        private readonly BoardModel _boardModel;

        private int _score;

        public ScoreModel(BoardModel boardModel)
        {
            _boardModel = boardModel;
        }

        public void HandleMatch(int count)
        {
            Score += count * 2;
        }
    }
}
