using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static Top_Down_Game.Game1;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static Microsoft.Xna.Framework.Color;

namespace Top_Down_Game
{
    internal class Projectile
    {
        private Texture2D _texture;
        private Vector2 _position;
        private Vector2 _target;
        private Rectangle _collision;
        private Rectangle _rectangle;
        private List<Tile> _obstacles;
        private List<Enemy> _enemies;
        public bool Dead { get; private set; }
        public bool KilledEnemy { get; private set; }

        private int _damage;
        private int _duration;

        private float xDiff;
        private float yDiff;
        private float xProjectileSpeed;
        private float yProjectileSpeed;

        public Projectile(ContentManager content, int damage, List<Tile> obstacles, List<Enemy> enemies, Vector2 position, Vector2 target)
        {
            _texture = content.Load<Texture2D>("Player/projectile");
            _damage = damage;
            _duration = 300;

            Dead = false;

            _position = position;
            _target = target;
            _target.X += _position.X - WINDOW_WIDTH / 2;
            _target.Y += _position.Y - WINDOW_HEIGHT / 2;

            _collision.X = (int)_position.X;
            _collision.Y = (int)_position.Y;
            _collision.Width = _texture.Width;
            _collision.Height = _texture.Height;

            _rectangle.X = 0;
            _rectangle.Y = 0;
            _rectangle.Width = _texture.Width;
            _rectangle.Height = _texture.Height;

            _obstacles = obstacles;

            _enemies = enemies;

            xDiff = _target.X - _position.X;
            yDiff = _target.Y - _position.Y;
            xProjectileSpeed = xDiff / _duration;
            yProjectileSpeed = yDiff / _duration;
        }
        public void Update()
        {
            KilledEnemy = false;

            bool collision = false;

            _position.X += xProjectileSpeed * 5;
            _position.Y += yProjectileSpeed * 5;

            _collision.X = (int)_position.X;
            _collision.Y = (int)_position.Y;

            foreach (var tile in _obstacles)
            {
                if (tile.Collision.Intersects(_collision)) collision = true;
            }

            if (!collision)
            {
                foreach (var enemy in _enemies)
                {
                    if (enemy.Collision.Intersects(_collision))
                    {
                        collision = true;
                        enemy.HP -= _damage;
                        if (enemy.HP <= 0)
                        {
                            KilledEnemy = true;
                            enemy.Dead = true;
                        }
                    }
                }
            }

            _duration--;
            if (_duration <= 0 || collision) Dead = true;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, _rectangle, White);
        }
    }
}
