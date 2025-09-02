using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private Image _image;

        public void Setup(Color color)
        {
            _image.color = color;
        }
    }
}
