using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using static Top_Down_Game.TileType;
using static Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework.Content;
namespace Top_Down_Game
{
    internal class Tile
    {
        public const int TILE_SIZE = 400;
        private Texture2D _texture;
        private Rectangle _rectangle;
        private Vector2 _position;
        private Rectangle _collision;
        public bool Solid { get; }
        public Rectangle Collision => _collision;
        public TileType TileType { get; }
        public Tile(TileType tileType, ContentManager content, int startX, int startY, bool solid)
        {
            TileType = tileType;

            if (TileType == Forest) _texture = content.Load<Texture2D>("TileTextures/grass");
            else if (TileType == Tree) _texture = content.Load<Texture2D>("TileTextures/tree");
            else if (TileType == Mountain) _texture = content.Load<Texture2D>("TileTextures/mountain");
            else if (TileType == Rock) _texture = content.Load<Texture2D>("TileTextures/rock");
            else if (TileType == Wetland) _texture = content.Load<Texture2D>("TileTextures/water");
            else if (TileType == WaterRock) _texture = content.Load<Texture2D>("TileTextures/waterRock");
            else if (TileType == Hotlands) _texture = content.Load<Texture2D>("TileTextures/lava");
            else if (TileType == LavaRock) _texture = content.Load<Texture2D>("TileTextures/lavaRock");

            _rectangle.X = 0;
            _rectangle.Y = 0;
            _rectangle.Width = _texture.Width;
            _rectangle.Height = _texture.Height;

            _position.X = startX;
            _position.Y = startY;

            _collision.X = (int)_position.X;
            _collision.Y = (int)_position.Y;
            _collision.Width = TILE_SIZE + 50;
            _collision.Height = TILE_SIZE + 50;

            Solid = solid;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, _rectangle, White);
            if (Solid) spriteBatch.Draw(_texture, new Vector2(_collision.X, _collision.Y), _rectangle, Blue);
        }
    }
}