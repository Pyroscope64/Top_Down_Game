using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using static System.IO.File;
using static Top_Down_Game.EnemyType;
using static Top_Down_Game.Map;

namespace Top_Down_Game
{
    internal class Enemy
    {
        protected Texture2D _texture;
        protected Rectangle _rectangle;
        protected Rectangle _collision;
        public Rectangle Collision => _collision;
        protected Vector2 _position;
        protected Vector2 _velocity;
        protected int _hp;
        protected int _damage;
        protected int _movementSpeed;
        protected EnemyType _type;
        protected bool _dead;
        public bool Dead => _dead;
        private List<Tile> _tiles;
        public int HP { get => _hp; set => _hp = value; }
        public int Damage { get => _damage; set => _damage = value; }
        public List<Enemy> OtherEnemies { get; set; }
        public Enemy(ContentManager content, int x, int y, EnemyType type, List<Tile> tiles)
        {
            _texture = content.Load<Texture2D>("Enemy/enemy");
            int[] enemyValues = ReadAllLines("Content/Enemy/enemy.txt").ToIntArray();
            _hp = enemyValues[0];
            _damage = enemyValues[1];
            _movementSpeed = enemyValues[2];
            _type = type;

            if (type == Mountain)
            {
                _hp *= 2;
                _damage = (int)(_damage * 1.5);
                _movementSpeed /= 2;

            }
            else if (type == Wetland)
            {
                _hp *= 2;
                _damage = (int)(_damage / 1.5);
                _movementSpeed /= 2;
            }
            else if (type == Hotland)
            {
                _hp *= 3;
                _damage *= 2;
            }

            _position.X = x;
            _position.Y = y;

            _rectangle.X = 0;
            _rectangle.Y = 0;
            _rectangle.Width = _texture.Width;
            _rectangle.Height = _texture.Height;

            _collision.X = (int)_position.X;
            _collision.Y = (int)_position.Y;
            _collision.Width = _texture.Width;
            _collision.Height = _texture.Height;

            _tiles = tiles;

            OtherEnemies = new List<Enemy>();
        }
        public void Update(int playerX, int playerY)
        {
            if (_hp <= 0) _dead = true;
            PathFind(playerX, playerY);
            GetVelocity();
            _position.X += _velocity.X;
            _position.Y += _velocity.Y;
            _collision.X = (int)_position.X - _texture.Width;
            _collision.Y = (int)_position.Y - _texture.Height;
        }
        private void GetVelocity()
        {
            if (_position.X + _velocity.X < 0) _velocity.X = -_position.X;
            else if (_position.X + _velocity.X > MAP_SIZE * Tile.TILE_SIZE) _velocity.X = MAP_SIZE * Tile.TILE_SIZE - _position.X;

            if (_position.Y + _velocity.Y < 0) _velocity.Y = -_position.Y;
            else if (_position.Y + _velocity.Y > MAP_SIZE * Tile.TILE_SIZE) _velocity.Y = MAP_SIZE * Tile.TILE_SIZE - _position.Y;

            foreach (Tile tile in _tiles.Where(tile => tile.Solid))
            {
                if (_position.X + _velocity.X < tile.Collision.X + Tile.TILE_SIZE + _texture.Width &&
                    _position.X > tile.Collision.X + 1 &&
                    _position.Y > tile.Collision.Y && _position.Y < tile.Collision.Y + Tile.TILE_SIZE + _texture.Height)
                {
                    _velocity.X = -(_position.X - (tile.Collision.X + Tile.TILE_SIZE + _texture.Width));
                }

                if (_position.X + _velocity.X > tile.Collision.X &&
                    _position.X < tile.Collision.X + Tile.TILE_SIZE - 1 &&
                    _position.Y > tile.Collision.Y && _position.Y < tile.Collision.Y + Tile.TILE_SIZE + _texture.Height)
                {
                    _velocity.X = tile.Collision.X - _position.X;
                }

                if (_position.Y + _velocity.Y < tile.Collision.Y + Tile.TILE_SIZE + _texture.Height &&
                    _position.Y > tile.Collision.Y + 1 &&
                    _position.X > tile.Collision.X && _position.X < tile.Collision.X + Tile.TILE_SIZE + _texture.Width)
                {
                    _velocity.Y = -(_position.Y - (tile.Collision.Y + Tile.TILE_SIZE + _texture.Height));
                }

                if (_position.Y + _velocity.Y > tile.Collision.Y &&
                    _position.Y < tile.Collision.Y + Tile.TILE_SIZE - 1 &&
                    _position.X > tile.Collision.X && _position.X < tile.Collision.X + Tile.TILE_SIZE + _texture.Width)
                {
                    _velocity.Y = tile.Collision.Y - _position.Y;
                }
            }
        }
        public void PathFind(int playerX, int playerY)
        {
            if (_position.X < playerX)
            {
                _velocity.X = _movementSpeed;
            }
            else if (_position.X > playerX)
            {
                _velocity.X = -_movementSpeed;
            }

            if (_position.Y < playerY)
            {
                _velocity.Y = _movementSpeed;
            }
            else if (_position.Y > playerY)
            {
                _velocity.Y = -_movementSpeed;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 DrawPos;
            DrawPos.X = _collision.X;
            DrawPos.Y = _collision.Y;
            spriteBatch.Draw(_texture, DrawPos, _rectangle, Color.White);
        }
    }
}
