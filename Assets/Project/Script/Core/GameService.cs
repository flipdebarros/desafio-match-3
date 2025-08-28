using System.Collections.Generic;
using Gazeus.DesafioMatch3.Controllers;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public class GameService
    {
        public BoardModel BoardModel;
        
        public GameService()
        {
            CreateModels();
        }

        public void Initialize()
        {
            InitializeModels();
        }

        public void Dispose()
        {
            DisposeModels();
        }

        private void CreateModels()
        {
            BoardModel = new BoardModel();
        }

        private void InitializeModels()
        {
            BoardModel.Initialize();
        }

        private void DisposeModels()
        {
            
        }
    }
}
