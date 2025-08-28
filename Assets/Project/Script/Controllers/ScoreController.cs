using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class ScoreController
    {
        private readonly ScoreModel _model;
        private readonly ScoreView _view;
        private readonly BoardController _boardController;

        public ScoreController(ScoreModel model, ScoreView view, BoardController boardController)
        {
            _model = model;
            _view = view;
            _boardController = boardController;
        }

        public void Initialize()
        {
            AddListeners();
            SetupView();
        }

        public void Dispose()
        {
            RemoveListeners();
        }
        
        private void AddListeners()
        {
            _model.OnScoreChanged += HandleScoreChanged;
            _boardController.OnMatch += _model.HandleMatch;
        }

        private void RemoveListeners()
        {
            _model.OnScoreChanged -= HandleScoreChanged;
            _boardController.OnMatch -= _model.HandleMatch;
        }

        private void SetupView()
        {
            _view.Setup($"Score: {_model.Score}");
        }

        private void HandleScoreChanged() => SetupView();
    }
}
