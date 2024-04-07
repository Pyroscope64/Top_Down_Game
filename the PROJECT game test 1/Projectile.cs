using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static Microsoft.Xna.Framework.Color;
using static Top_Down_Game.Game1;
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
        public bool KilledEnemy { get; private set; } // Whether or not the projectile has defeated an enemy

        private int _damage;
        private int _duration; // How long the projectile will travel for

        private float xDiff; // The difference in X to the current position and the destination
        private float yDiff; // The difference in Y
        private float xProjectileSpeed; // The speed at which the projectile is moving on the X axis
        private float yProjectileSpeed; // The speed on the Y axis
        public Projectile(ContentManager content, int damage, List<Tile> obstacles, List<Enemy> enemies, Vector2 position, Vector2 target)
        {
            _texture = content.Load<Texture2D>("Player/projectile");
            _damage = damage;
            _duration = 300; // The duration for the projectile will be 5 seconds

            Dead = false; // Assume it is not already dead

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
            KilledEnemy = false; // Assume we have not already defeated an enemy

            bool collision = false; // Assume we have not already collided with a tile

            _position.X += xProjectileSpeed * 5;
            _position.Y += yProjectileSpeed * 5;

            _collision.X = (int)_position.X;
            _collision.Y = (int)_position.Y;

            foreach (var tile in _obstacles) // For each solid tile in the map
            {
                if (tile.Collision.Intersects(_collision))
                {
                    collision = true;
                    break;
                } // If the projectile collided with a solid tile, it disappears
            }

            if (!collision) // If there has not already been a collision with a solid tile
            {
                foreach (var enemy in _enemies) // For every enemy in the game
                {
                    if (enemy.Collision.Intersects(_collision)) // If the projectile collides with an enemy
                    {
                        collision = true; // It has collided
                        enemy.HP -= _damage; // The enenmy's HP decreases according to the player's damage
                        if (enemy.HP <= 0) // If this projectile defeated an enemy
                        {
                            KilledEnemy = true; // Then we say that it has defeated an enemy
                            enemy.Dead = true; // And the enemy disappears
                        }
                    }
                }
            }

            _duration--; // The duration decreases as the projectile continues to move
            if (_duration <= 0 || collision) Dead = true; // If the projectile has collided with something or has travelled for too long, it disappears
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, _rectangle, White); // Draw the projectile
        }
    }
}
