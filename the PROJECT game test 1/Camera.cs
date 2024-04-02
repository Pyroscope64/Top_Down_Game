using Microsoft.Xna.Framework;
using static Microsoft.Xna.Framework.Matrix;
namespace Top_Down_Game
{
    internal class Camera
    {
        private Vector2 _resolution; 
        private Vector2 _halfResolution;
        private Matrix _transform;
        public Matrix Transform { get => _transform; }
        public Camera(Point resolution)
        {
            _resolution = resolution.ToVector2();
            _halfResolution = _resolution / 2;
        }
        public void Follow(Vector2 target) // Moves the camera accordingly with the player's movement
        {
            Matrix position = CreateTranslation(-target.X, -target.Y, 0);
            Matrix offset = CreateTranslation(_halfResolution.X, _halfResolution.Y, 0);
            _transform = position * offset;
        }
    }
}