using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static System.IO.File;
using static System.Convert;
using static Top_Down_Game.EnemyType;
using static Top_Down_Game.Map;
using static Top_Down_Game.Tile;
using static Top_Down_Game.Game1;
namespace Top_Down_Game
{
    internal class Enemy
    {
        private Texture2D _texture;
        private Rectangle _rectangle;
        private Rectangle _collision;
        public Rectangle Collision => _collision; // The enemy's collision rectangle
        private Vector2 _position; // The enemy's current position
        private Vector2 _velocity; // The x and y speeds of the enemy
        private int _movementSpeed; // The speed that the enemy will move towards the player at
        private EnemyType _type; // The type of enemy, e.g. forest, mountain, etc
        public bool Dead { get; set; } // Whether or not the enemy has lost all of its health points
        private List<Tile> _tiles;
        public int HP { get; set; } // The number of health points that the enemy has
        public int Damage { get; set; } // The number of health points that will be taken away from the player upon collision
        public List<Enemy> OtherEnemies { get; set; } // The list of other enemies currently in the game
        private int _maxAttackCooldown; // The maximum amount of time before an enemy can damage the player
        private int _attackCooldown; // The current time remaining before the enemy can damage the player
        public bool CanAttack { get; private set; } // Whether or not the enemy can damage the player
        private int _maxPathFindCooldown; // The maximum time to wait before calculating the path to the player
        private int _pathFindCooldown; // The current time remaining before the enemy calculates the path to the player
        private int _bossMaxHP; // The final boss' maximum health points
        private Texture2D _bossHealthBar; // The health bar which shows how much health the boss has left
        private Rectangle _bossHealthBarRect;
        private Vector2 _bossHealthBarPos;
        private SpriteFont _font;
        private List<Node> _path; // The path of nodes from the enemy's current position, and the player's position
        public Enemy(ContentManager content, int x, int y, EnemyType type, List<Tile> tiles)
        {
            string file = type == Boss ? "boss" : "enemy"; // If the enemy is the boss, read the boss file, otherwise read the enemy file
            int[] enemyValues = ReadAllLines($"Content/Enemy/{file}.txt").ToIntArray();
            HP = enemyValues[0];
            Damage = enemyValues[1];
            _movementSpeed = enemyValues[2];
            _maxAttackCooldown = enemyValues[3];
            _pathFindCooldown = _maxPathFindCooldown;
            _type = type;
            if (_type != Boss) _maxPathFindCooldown = enemyValues[4];
            _attackCooldown = _maxAttackCooldown;

            if (_type == Forest) file = "forestEnemy"; // Gets the texture for the specific type of enemy
            else if (type == Mountain) file = "mountainEnemy";
            else if (type == Wetland) file = "waterEnemy";
            else if (type == Hotland) file = "lavaEnemy";
            _texture = content.Load<Texture2D>($"Enemy/{file}");

            if (type == Mountain) // Different types of enemies will have different values for health, damage, etc
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

            if (_type == Boss) // If the enemy is a boss, then we should display the boss' health bar
            {
                _bossHealthBar = content.Load<Texture2D>("Enemy/bossHealthBar");
                 
                _bossHealthBarRect.X = 0;
                _bossHealthBarRect.Y = 0;
                _bossHealthBarRect.Width = _bossHealthBar.Width;
                _bossHealthBarRect.Height = _bossHealthBar.Height;
                _font = content.Load<SpriteFont>("Fonts/Font");
            }

            _path = new List<Node>();
        }
        public void Update(Rectangle playerCollision)
        {
            if (HP <= 0) Dead = true; // If the enemy has lost all health points, it should disappear
            Attack(playerCollision.X, playerCollision.Y); // Handles moving towards the player
            GetVelocity(); // Checks for collisions

            if (_attackCooldown < _maxAttackCooldown) // If the enemy has already attacked a player
            {
                CanAttack = false; // It cannot attack again 
                _attackCooldown--; // We decrease the current cooldown
                if (_attackCooldown <= 0) _attackCooldown = _maxAttackCooldown; // until the enemy can attack again
            }

            if (Collision.Intersects(playerCollision)) // If the enemy is touching the player
            {
                if (_attackCooldown == _maxAttackCooldown) // and the enemy can attack
                {
                    CanAttack = true; 
                    _attackCooldown--; // then we start reducing the cooldown again
                }
            }

            _position.X += _velocity.X;
            _position.Y += _velocity.Y;
            _collision.X = (int)_position.X - _texture.Width;
            _collision.Y = (int)_position.Y - _texture.Height;

            if (_type == Boss) // If the enemy is a boss, update the health bar's position and width
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
            _pathFindCooldown--; // Reduce the pathfinding cooldown
            if (_pathFindCooldown <= 0) // If it gets to 0
            {
                _pathFindCooldown = _maxPathFindCooldown; // set it to the max, so we can look for the player again
            }
             
            if (_pathFindCooldown == _maxPathFindCooldown) // If we are able to find a path
            {
                PathFind(playerX, playerY); // then we attempt to find a path
            }

            Rectangle radius = new Rectangle(); // The radius in which the enemy should stop pathfinding

            radius.X = playerX - TILE_SIZE;
            radius.Width = playerX + TILE_SIZE - radius.X;

            radius.Y = playerY - TILE_SIZE;
            radius.Height = playerY + TILE_SIZE - radius.Y;

            if (Collision.Intersects(radius)) MoveTo(playerX, playerY); // If we are close to the player, follow their position directly
            else FollowPath(playerX, playerY); // Otherwise, we follow the path that the pathfinding algorithm found
        }
        private void FollowPath(int playerX, int playerY)
        {
            int currentlyAt = _path.Count - 1; // Assume we are currently at the end of the list
            Node goingTo = new Node(playerX, playerY); // Assume the node we are going to is the player's node
            if (_path.Count > 1) // If there are multiple things in the list
            {
                goingTo = _path[currentlyAt - 1]; // Then we are going to the node in the list before our current node
                _path.Remove(_path[_path.Count - 1]); // Remove the current node so we do not go to it again
            }

            if (_path.Count > 2) // If there are more than 2 things in the path
            {
                for (int i = 0; i < _path.Count; i++) // Then we remove every odd node to prevent zigzag motions, and allow for diagonal movement
                {
                    if (i % 2 == 1) _path.Remove(_path[i]);
                }
            }

            if (currentlyAt >= 1) // If we are 1 or more nodes away from the player
            {
                if (Collision.X == goingTo.X && Collision.Y == goingTo.Y) // If we have arrived at the next node in the list
                {
                    currentlyAt--; // Then we will start moving to the next one
                    goingTo = _path[currentlyAt - 1]; // We are now going to the next node in the list
                }
                MoveTo(goingTo.X, goingTo.Y); // So we move towards this node
            }
            else MoveTo(goingTo.X, goingTo.Y); // Otherwise, we move towards the node we are currently looking at
        }
        private void PathFind(int playerX, int playerY)
        {
            List<Node> open = new List<Node>()
            {
                new Node((int)_position.X, (int)_position.Y) // Add the current position to the open list
            };
            List<Node> closed = new List<Node>(); 
             
            Node current = new Node((int)_position.X, (int)_position.Y); // Our current position is our current node
            Node end = new Node(playerX, playerY); // The player's position is where we are trying to get to

            int g = 0; // g = distance travelled

            while (open.Count > 0) // While there are still nodes in the open list
            {
                var lowest = open.Min(l => l.F); // f is the heuristic distance between the enemy and the player
                current = open.First(l => l.F == lowest); // Get the node with the lowest f value

                closed.Add(current); // Add the current node to the closed list so we do not look at it again
                open.Remove(current); // Remove it from the open list as we have already seen it

                List<Tile> tiles = _tiles.Where(tile =>  tile.Collision.Intersects(new Rectangle(playerX,
                                                        playerY,
                                                        Collision.Width,
                                                        Collision.Height))).ToList(); // Gets the tile which the player is currently on
                bool playerTile = new Rectangle(current.X, current.Y, Collision.Width, Collision.Height).Intersects(tiles[0].Collision); 
                // Returns true if the player is currently on the same tile as the enemy

                if (playerTile) // If we have found the destination
                {
                    while(true) // Loop back to the beginning
                    {
                        current = current.Parent; // And add every parent node to the list
                        if (current == null) return; // If our current node is null (i.e. if the loop started with the enemy and player on the same node
                        if (current.Parent == null) return; // If we have reached the beginning of the list
                        _path.Add(current); // Add the current node to the path
                    }
                }

                List<Node> adjacents = new List<Node>(); // All the tiles which are adjacent to the enemy's current tile
                adjacents = GetWalkableAdjacents(adjacents, current); // Returns the tiles which are not solid
                g++; // Increment g, as we have travelled one place

                foreach (Node adjacent in adjacents) // Look at each adjacent node
                {
                    if (closed.FirstOrDefault(l => l.X == adjacent.X && l.Y == adjacent.Y) != null) continue; // If it's in the closed list, move to the next
                    if (open.FirstOrDefault(l => l.X == adjacent.X && l.Y == adjacent.Y) == null) // If it is not in the open list...
                    {
                        adjacent.G = current.G + 1; 
                        adjacent.H = (int)CalculateH(adjacent, end);
                        adjacent.F = adjacent.G + adjacent.H; // ...Calculate the values...
                        adjacent.Parent = current;

                        open.Add(adjacent); // ...and add it to the open list
                    }
                    else // If it is already in the open list...
                    {
                        if (g + adjacent.H < adjacent.F) // ...Check if the new F value is better than the ones we already have for this node...
                        {
                            adjacent.G = current.G + 1;
                            adjacent.F = adjacent.G + adjacent.H; // ...and calculate the new F value
                            adjacent.Parent = current;
                        }
                    }
                }

                if (g >= 30) return; // If g is above 30, return (prevents freezing if the enemy is too far away from the player)
            }
        }
        private double CalculateH(Node Q, Node destination) => Math.Abs(destination.X - Q.X) + Math.Abs(destination.Y - Q.Y); // Calculates the heuristic distance between the enemy and the player
        private List<Node> GetWalkableAdjacents(List<Node> adjacents, Node current) // Returns a list of nodes that the enemy can currently move to
        {
            if (current.X < MAP_SIZE * TILE_SIZE) adjacents.Add(new Node(current.X + TILE_SIZE, current.Y));
            adjacents.Add(new Node(current.X - TILE_SIZE, current.Y));
            if (current.Y < MAP_SIZE * TILE_SIZE) adjacents.Add(new Node(current.X, current.Y + TILE_SIZE));
            adjacents.Add(new Node(current.X, current.Y - TILE_SIZE));
            return adjacents.Where(adjacent => !IsObstacle(adjacent)).ToList();
        }
        private bool IsObstacle(Node adjacent) // returns true if the adjacent tile to the enemy is a solid tile
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
        private void MoveTo(int endX, int endY) // Directly moves the enemy to the given position
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
            spriteBatch.Draw(_texture, DrawPos, _rectangle, Color.White); // Draws the enemy to the screen

            if (_type == Boss) // Draws the boss' health bar to the screen, if the enemy is a boss
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