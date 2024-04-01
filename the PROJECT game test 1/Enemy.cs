using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using static System.IO.File;
using static System.Convert;
using static Top_Down_Game.EnemyType;
using static Top_Down_Game.Map;
using static Top_Down_Game.Tile;
using static Top_Down_Game.Game1;
using System;

namespace Top_Down_Game
{
    internal class Enemy
    {
        private Texture2D _texture;
        private Rectangle _rectangle;
        private Rectangle _collision;
        public Rectangle Collision => _collision;
        private Vector2 _position;
        private Vector2 _velocity;
        private int _movementSpeed;
        private EnemyType _type;

        public bool Dead { get; set; }
        private List<Tile> _tiles;
        public int HP { get; set; }
        public int Damage { get; set; }
        public List<Enemy> OtherEnemies { get; set; }
        private int _maxAttackCooldown;
        private int _attackCooldown;
        public bool CanAttack { get; private set; }
        private int _maxPathFindCooldown;
        private int _pathFindCooldown;

        private int _bossMaxHP;
        private Texture2D _bossHealthBar;
        private Rectangle _bossHealthBarRect;
        private Vector2 _bossHealthBarPos;
        private SpriteFont _font;

        private List<Node> _path;
        public Enemy(ContentManager content, int x, int y, EnemyType type, List<Tile> tiles)
        {
            string file = type == Boss ? "boss" : "enemy";
            int[] enemyValues = ReadAllLines($"Content/Enemy/{file}.txt").ToIntArray();
            HP = enemyValues[0];
            Damage = enemyValues[1];
            _movementSpeed = enemyValues[2];
            _maxAttackCooldown = enemyValues[3];
            _pathFindCooldown = _maxPathFindCooldown;
            _type = type;
            if (_type != Boss) _maxPathFindCooldown = enemyValues[4];
            _attackCooldown = _maxAttackCooldown;

            if (_type == Forest) file = "forestEnemy";
            else if (type == Mountain) file = "mountainEnemy";
            else if (type == Wetland) file = "waterEnemy";
            else if (type == Hotland) file = "lavaEnemy";
            _texture = content.Load<Texture2D>($"Enemy/{file}");

            if (type == Mountain)
            {
                HP *= 2;
                Damage = (int)(Damage * 1.5);
                _movementSpeed /= 2;

            }
            else if (type == Wetland)
            {
                HP *= 2;
                Damage = (int)(Damage / 1.5);
                _movementSpeed /= 2;
            }
            else if (type == Hotland)
            {
                HP *= 3;
                Damage *= 2;
            }

            _bossMaxHP = HP;

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

            if (_type == Boss)
            {
                _bossHealthBar = content.Load<Texture2D>("Enemy/bossHealthBar");
                 
                _bossHealthBarRect.X = 0;
                _bossHealthBarRect.Y = 0;
                _bossHealthBarRect.Width = _bossHealthBar.Width;
                _bossHealthBarRect.Height = _bossHealthBar.Height;
            }

            if (_type == Boss)
            {
                _font = content.Load<SpriteFont>("Fonts/Font");
            }

            _path = new List<Node>();
        }
        public void Update(Rectangle playerCollision)
        {
            if (HP <= 0) Dead = true;
            Attack(playerCollision.X, playerCollision.Y);
            GetVelocity();

            if (_attackCooldown < _maxAttackCooldown)
            {
                CanAttack = false;
                _attackCooldown--;
                if (_attackCooldown <= 0) _attackCooldown = _maxAttackCooldown;
            }

            if (Collision.Intersects(playerCollision))
            {
                if (_attackCooldown == _maxAttackCooldown)
                {
                    CanAttack = true;
                    _attackCooldown--;
                }
            }

            _position.X += _velocity.X;
            _position.Y += _velocity.Y;
            _collision.X = (int)_position.X - _texture.Width;
            _collision.Y = (int)_position.Y - _texture.Height;

            if (_type == Boss)
            {
                _bossHealthBarPos.X = playerCollision.X - _bossHealthBar.Width / 2;
                _bossHealthBarPos.Y = playerCollision.Y + WINDOW_HEIGHT / 2 - _bossHealthBar.Height - 30;
                _bossHealthBarRect.Width = (int)(_bossHealthBar.Width * (ToDecimal(HP) / ToDecimal(_bossMaxHP)));
            }
        }
        private void GetVelocity()
        {
            if (_position.X + _velocity.X < 0) _velocity.X = -_position.X;
            else if (_position.X + _velocity.X > MAP_SIZE * TILE_SIZE) _velocity.X = MAP_SIZE * TILE_SIZE - _position.X;

            if (_position.Y + _velocity.Y < 0) _velocity.Y = -_position.Y;
            else if (_position.Y + _velocity.Y > MAP_SIZE * TILE_SIZE) _velocity.Y = MAP_SIZE * TILE_SIZE - _position.Y;

            foreach (Tile tile in _tiles.Where(tile => tile.Solid))
            {
                if (_position.X + _velocity.X < tile.Collision.X + TILE_SIZE + _texture.Width &&
                    _position.X > tile.Collision.X + 1 &&
                    _position.Y > tile.Collision.Y && _position.Y < tile.Collision.Y + TILE_SIZE + _texture.Height)
                {
                    _velocity.X = -(_position.X - (tile.Collision.X + TILE_SIZE + _texture.Width));
                }

                if (_position.X + _velocity.X > tile.Collision.X &&
                    _position.X < tile.Collision.X + TILE_SIZE - 1 &&
                    _position.Y > tile.Collision.Y && _position.Y < tile.Collision.Y + TILE_SIZE + _texture.Height)
                {
                    _velocity.X = tile.Collision.X - _position.X;
                }

                if (_position.Y + _velocity.Y < tile.Collision.Y + TILE_SIZE + _texture.Height &&
                    _position.Y > tile.Collision.Y + 1 &&
                    _position.X > tile.Collision.X && _position.X < tile.Collision.X + TILE_SIZE + _texture.Width)
                {
                    _velocity.Y = -(_position.Y - (tile.Collision.Y + TILE_SIZE + _texture.Height));
                }

                if (_position.Y + _velocity.Y > tile.Collision.Y &&
                    _position.Y < tile.Collision.Y + TILE_SIZE - 1 &&
                    _position.X > tile.Collision.X && _position.X < tile.Collision.X + TILE_SIZE + _texture.Width)
                {
                    _velocity.Y = tile.Collision.Y - _position.Y;
                }
            }

            foreach (Enemy enemy in OtherEnemies) // when close to other enemy it jerks back a certain distance
            {
                if (_position.X + _velocity.X < enemy.Collision.X + _texture.Width * 2 && // if where we are in the next second 
                    _position.X > enemy.Collision.X + 1 &&
                    _position.Y > enemy.Collision.Y && _position.Y < enemy.Collision.Y + _texture.Height * 2)
                {
                    _velocity.X = 0;
                }

                if (_position.X + _velocity.X > enemy.Collision.X &&
                    _position.X < enemy.Collision.X + _texture.Width * 2 - 1 &&
                    _position.Y > enemy.Collision.Y && _position.Y < enemy.Collision.Y + _texture.Height * 2)
                {
                    _velocity.X = 0;
                }

                if (_position.Y + _velocity.Y < enemy.Collision.Y + _texture.Height * 2 &&
                    _position.Y > enemy.Collision.Y + 1 &&
                    _position.X > enemy.Collision.X && _position.X < enemy.Collision.X + _texture.Width * 2)
                {
                    _velocity.Y = 0;
                }

                if (_position.Y + _velocity.Y > enemy.Collision.Y &&
                    _position.Y < enemy.Collision.Y + _texture.Height * 2 - 1 &&
                    _position.X > enemy.Collision.X && _position.X < enemy.Collision.X + _texture.Width * 2)
                {
                    _velocity.Y = 0;
                }
            }
        }
        public void Attack(int playerX, int playerY)
        {
            _pathFindCooldown--;
            if (_pathFindCooldown <= 0)
            {
                _pathFindCooldown = _maxPathFindCooldown;
            }

            if (_pathFindCooldown == _maxPathFindCooldown)
            {
                PathFind(playerX, playerY);
            }

            Rectangle radius = new Rectangle();

            radius.X = playerX - TILE_SIZE;
            radius.Width = playerX + TILE_SIZE - radius.X;

            radius.Y = playerY - TILE_SIZE;
            radius.Height = playerY + TILE_SIZE - radius.Y;

            if (Collision.Intersects(radius)) MoveTo(playerX, playerY);
            else FollowPath(playerX, playerY);
        }
        private void FollowPath(int playerX, int playerY)
        {
            int currentlyAt = _path.Count - 1;
            Node goingTo = new Node(playerX, playerY);
            if (_path.Count > 1)
            {
                goingTo = _path[currentlyAt - 1];
                _path.Remove(_path[_path.Count - 1]);
            }

            if (_path.Count > 2)
            {
                for (int i = 0; i < _path.Count; i++)
                {
                    if (i % 2 == 1) _path.Remove(_path[i]);
                }
            }

            if (currentlyAt >= 1)
            {
                MoveTo(goingTo.X, goingTo.Y);
                if (Collision.X == goingTo.X && Collision.Y == goingTo.Y)
                {
                    currentlyAt--;
                    goingTo = _path[currentlyAt - 1];
                }
            }
            else MoveTo(goingTo.X, goingTo.Y);
        }
        private void PathFind(int playerX, int playerY)
        {
            List<Node> open = new List<Node>()
            {
                new Node((int)_position.X, (int)_position.Y)
            };
            List<Node> closed = new List<Node>();

            Node current = new Node((int)_position.X, (int)_position.Y);
            Node end = new Node(playerX, playerY);

            int g = 0;

            while (open.Count > 0)
            {
                var lowest = open.Min(l => l.F);
                current = open.First(l => l.F == lowest);

                closed.Add(current); // ADD IT TO CLOSED LIST SO WE DON'T LOOK AT IT AGAIN
                open.Remove(current); // REMOVE IT FROM OPEN LIST BECAUSE WE'VE ALREADY SEEN IT

                List<Tile> tiles = _tiles.Where(tile => tile.Collision.Intersects(new Rectangle(playerX, playerY, Collision.Width, Collision.Height))).ToList();
                bool playerTile = new Rectangle(current.X, current.Y, Collision.Width, Collision.Height).Intersects(tiles[0].Collision);

                if (playerTile) // IF WE HAVE FOUND THE DESTINATION
                {
                    while(true) // ALL THE WAY BACK TO THE BEGINNING
                    {
                        current = current.Parent; // AND ADD EVERY PARENT TO THE PATH
                        if (current == null) return;
                        if (current.Parent == null) return;
                        _path.Add(current);
                    }
                }

                List<Node> adjacents = new List<Node>(); // ADJACENT NODES TO CURRENT
                adjacents = GetWalkableAdjacents(adjacents, current); // GETS THEM
                g++;

                foreach (Node adjacent in adjacents) // Look at each adjacent node
                {
                    if (closed.FirstOrDefault(l => l.X == adjacent.X && l.Y == adjacent.Y) != null) continue; // If it's in the closed list, move to the next
                    #region If it isn't in the open list, calculate the values and add it to the open list
                    if (open.FirstOrDefault(l => l.X == adjacent.X && l.Y == adjacent.Y) == null) // If it is not in the open list...
                    {
                        adjacent.G = current.G + 1;
                        adjacent.H = (int)CalculateH(adjacent, end);
                        adjacent.F = adjacent.G + adjacent.H; // ...CALCULATE VALUES...
                        adjacent.Parent = current;

                        open.Add(adjacent); // ...AND ADD TO THE OPEN LIST
                    }
                    #endregion
                    else // If it is already in the open list...
                    {
                        #region Check if the F value is better than the existing one, and update it if so
                        if (g + adjacent.H < adjacent.F) // ...CHECK IF ITS VALUES ARE BETTER THAN THE ONE WE ALREADY HAVE...
                        {
                            adjacent.G = current.G + 1;
                            adjacent.F = adjacent.G + adjacent.H; // ...AND CALCULATE THE NEW VALUES
                            adjacent.Parent = current;
                        }
                        #endregion
                    }
                }

                if (g >= 30) return;
            }
        }
        private double CalculateH(Node Q, Node destination) => Math.Abs(destination.X - Q.X) + Math.Abs(destination.Y - Q.Y);
        private List<Node> GetWalkableAdjacents(List<Node> adjacents, Node current)
        {
            if (current.X < MAP_SIZE * TILE_SIZE) adjacents.Add(new Node(current.X + TILE_SIZE, current.Y));
            adjacents.Add(new Node(current.X - TILE_SIZE, current.Y));
            if (current.Y < MAP_SIZE * TILE_SIZE) adjacents.Add(new Node(current.X, current.Y + TILE_SIZE));
            adjacents.Add(new Node(current.X, current.Y - TILE_SIZE));
            return adjacents.Where(adjacent => !IsObstacle(adjacent)).ToList();
        }
        private bool IsObstacle(Node adjacent)
        {
            Rectangle adjacentRect = new Rectangle();
            adjacentRect.X = adjacent.X;
            adjacentRect.Y = adjacent.Y;
            adjacentRect.Width = Collision.Width;
            adjacentRect.Height = Collision.Height;

            foreach (Tile tile in _tiles)
            {
                if (adjacentRect.Intersects(tile.Collision) && tile.Solid) 
                {
                    return true;
                }
            }
            return false;
        }
        private void MoveTo(int endX, int endY)
        {
            if (_position.X < endX)
            {
                _velocity.X = _movementSpeed;
            }
            else if (_position.X > endX)
            {
                _velocity.X = -_movementSpeed;
            }

            if (_position.Y < endY)
            {
                _velocity.Y = _movementSpeed;
            }
            else if (_position.Y > endY)
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

            if (_type == Boss)
            {
                spriteBatch.Draw(_bossHealthBar, _bossHealthBarPos, _bossHealthBarRect, Color.White);

                Vector2 hpDraw = _bossHealthBarPos;
                hpDraw.X = _bossHealthBarPos.X + 15;
                hpDraw.Y = _bossHealthBarPos.Y + 15;

                spriteBatch.DrawString(_font, $"{HP}/{_bossMaxHP}", hpDraw, Color.White);
            }
        }
    }
    enum EnemyType
    {
        Forest,
        Mountain,
        Wetland,
        Hotland,
        Boss
    }
}