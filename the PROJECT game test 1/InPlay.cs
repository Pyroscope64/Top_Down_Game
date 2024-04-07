using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static System.TimeSpan;
using static System.IO.File;
using static Top_Down_Game.Animation;
using static Top_Down_Game.InputHandler;
using static Top_Down_Game.EnemyType;
using static Top_Down_Game.Tile;
using static Microsoft.Xna.Framework.Graphics.BlendState;
using static Microsoft.Xna.Framework.Graphics.SpriteSortMode;
namespace Top_Down_Game
{
    internal class InPlay : GameState
    {
        public static bool AnimationTick; // Whether or not to advance the animation to the next frame
        private TimeSpan _elapsedTime = Zero;
        private const float _tickTime = 0.08f; // How often the game will tick

        private Player _player; // The player of the game
        private Map _map; // The map, including all tiles of the game
        private Camera _camera; // The camera which will follow the player

        private List<Enemy> _enemies; // All enemies in the game

        private EnemyType[] enemyTypes = new EnemyType[] // The 4 types of enemies, other than the boss
        {
            Forest,
            Mountain,
            Wetland,
            Hotland
        };

        private bool _bossIsAlive;
        public InPlay(ContentManager content, GraphicsDevice graphicsDevice, Game1 game, Camera camera) : base(content, graphicsDevice, game)
        {
            Dictionary<string, Animation> playerAnimations = CreatePlayerAnimations();
            _map = new Map(content);
            _enemies = new List<Enemy>();
            _player = new Player(
                content.Load<Texture2D>("Player/HeroKnight"),
                content,
                graphicsDevice,
                new Vector2(1000, 1000),
                playerAnimations,
                game,
                _map,
                _enemies);
            _camera = camera;
            _bossIsAlive = false;
        }
        private Dictionary<string, Animation> CreatePlayerAnimations()
        {
            Dictionary<string, Animation> playerAnimations = new Dictionary<string, Animation>();
            Point playerSpriteSize = new Point(100, 55);
            Point spriteSheetSize = new Point(1000, 495);
            playerAnimations.Add("Idle", Create(playerSpriteSize, spriteSheetSize, 0, 7, true));
            playerAnimations.Add("Run", Create(playerSpriteSize, spriteSheetSize, 8, 17, true));
            playerAnimations.Add("Hurt", Create(playerSpriteSize, spriteSheetSize, 45, 47, false));
            return playerAnimations;
        }
        private void Tick(GameTime gameTime) // Advances the player animatino to the next frame
        {
            _elapsedTime += gameTime.ElapsedGameTime;
            if (_elapsedTime.TotalSeconds >= _tickTime)
            {
                AnimationTick = true;
                _elapsedTime = Zero;
            }
            else AnimationTick = false;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.End();

            spriteBatch.Begin(Deferred, AlphaBlend, null, null, null, null, _camera.Transform); // Used for full camera

            _map.Draw(spriteBatch); // Draw each tile to the screen
            _player.Draw(spriteBatch); // Draw the player
            if (_enemies.Count > 0) // Draw every enemy
            {
                foreach (Enemy enemy in _enemies)
                {
                    enemy.Draw(spriteBatch);
                }
            }
            if (_bossIsAlive) _enemies[0].Draw(spriteBatch); // If there is a boss, there is only 1 enemy (the boss) therefore we draw only that
        }

        public override void Update(GameTime gameTime)
        {
            InputHandler.Update(); // Gets the current inputs
            Tick(gameTime); // Ticks the game

            int questNum = _player.QuestNum; // Gets the players quest number
            _player.Update(); // Then updates the player
            if (_player.QuestNum > questNum && _player.QuestNum < 5) // If the player has completed a quest
            {
                string[] lines = ReadAllLines($"Content/Enemy/enemyLocations{_player.QuestNum}.txt"); // Then we load the enemies for the next quest
                for (int i = 0; i < lines.Length; i++)
                {
                    int[] location = lines[i].Split(',').ToIntArray();
                    _enemies.Add(new Enemy(_content, location[0] * TILE_SIZE + 50, location[1] * TILE_SIZE + 50, enemyTypes[questNum], _map.Tiles));
                }
                for (int i = 0; i < _enemies.Count; i++) // We get the list of other enemies to the current enemy, in order to implement collision
                {
                    Enemy enemy = _enemies[i];
                    foreach (Enemy enemy2 in _enemies)
                    {
                        enemy.OtherEnemies.Add(enemy2);
                    }
                    enemy.OtherEnemies.Remove(enemy);
                }
            }

            if (_player.QuestNum == 5 && _player.SpawnBoss) // If the boss should spawn in
            {
                _player.SpawnBoss = false; // We set this to false so that the boss only spawns once
                if (_enemies.Count == 0) // Prevents the boss from spawning in twice
                {
                    _enemies.Add(new Enemy(_content, (int)NPC.Position.X, (int)NPC.Position.Y, Boss, _map.Tiles));
                    _bossIsAlive = true; // The boss is now alive
                }
            }

            if (_bossIsAlive) // If the boss is still alive
            {
                _enemies[0].Update(_player.Collision); // Update it
                if (_enemies[0].CanAttack) // If the boss can attack (is touching the player)
                {
                    _player.HP -= _enemies[0].Damage; // Then we decrease the player's HP
                }
                if (_enemies[0].Dead) _player.QuestNum++; // If the boss has died, the player has completed the quest
            }
            else UpdateEnemies(); // Otherwise, we update the enemies

            _camera.Follow(_player.Position); // Follows the player

            UpdatePrevious(); // Updates the previous game state
        }
        private void UpdateEnemies() 
        {
            if (_enemies.Count > 0) // Updates every enemy
            {
                for (int i = 0; i < _enemies.Count; i++)
                {
                    _enemies[i].Update(_player.Collision);
                    if (_enemies[i].CanAttack) // If the enemy is touching the player and can attack
                    {
                        _player.HP -= _enemies[i].Damage; // then we decrease the player's HP
                    }
                }
            }

            for (int i = 0; i < _enemies.Count; i++) // Check if any of the enemies have been defeated
            {
                if (_enemies[i].Dead) _enemies.Remove(_enemies[i]);
            }
        }
    }
}
