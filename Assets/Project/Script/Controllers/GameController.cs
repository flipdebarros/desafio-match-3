using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Views;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private int _boardHeight = 10;
        [SerializeField] private int _boardWidth = 10;

        [SerializeField] private ScoreView _scoreView;

        private GameService _gameEngine;
        private BoardController _boardController;
        private ScoreController _scoreController;

        #region Unity
        private void Awake()
        {
            _gameEngine = new GameService();
            CreateControllers();
        }

        private void OnDestroy()
        {
            DisposeControllers();
            _gameEngine.Dispose();
        }

        private void Start()
        {
            InitializeControllers();
            _gameEngine.Initialize();
        }
        #endregion

        private void CreateControllers()
        {
            _boardController = new BoardController(_gameEngine.BoardModel, _boardView, _boardWidth, _boardHeight);
            _scoreController = new ScoreController(_gameEngine.ScoreModel, _scoreView, _boardController);
        }

        private void InitializeControllers()
        {
            _boardController.Initialize();
            _scoreController.Initialize();
        }

        private void DisposeControllers()
        {
            _boardController.Dispose();
            _scoreController.Dispose();
        }
    }
}
