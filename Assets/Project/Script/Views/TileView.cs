using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private Image _image;

        public void Setup(Sprite sprite, Color color)
        {
            _image.color = color;
            _image.sprite = sprite;
            _image.preserveAspect = true;
        }
    }
}
