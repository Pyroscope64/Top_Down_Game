using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using static System.TimeSpan;
using static System.IO.File;
using static Top_Down_Game.Animation;
using static Top_Down_Game.InputHandler;
using static Top_Down_Game.EnemyType;
using static Microsoft.Xna.Framework.Graphics.BlendState;
using static Microsoft.Xna.Framework.Graphics.SpriteSortMode;

namespace Top_Down_Game
{
    internal class InPlay : GameState
    {
        public static bool AnimationTick;
        private TimeSpan _elapsedTime = Zero;
        private const float _tickTime = 0.08f;

        private Player _player;
        private Map _map;
        private Camera _camera;

        private List<Enemy> _enemies;

        private EnemyType[] enemyTypes = new EnemyType[] { Forest, Mountain, Wetland, Hotland };
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
                new Point(100, 55),
                playerAnimations,
                game,
                _map,
                _enemies);
            _camera = camera;
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
        private void Tick(GameTime gameTime)
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

            spriteBatch.Begin(Deferred, AlphaBlend, null, null, null, null, _camera.Transform); //used for full camera

            _map.Draw(spriteBatch);
            _player.Draw(spriteBatch);
            if (_enemies.Count > 0)
            {
                foreach (Enemy enemy in _enemies)
                {
                    enemy.Draw(spriteBatch);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            InputHandler.Update();
            Tick(gameTime);

            int questNum = _player.QuestNum;
            _player.Update();
            if (_player.QuestNum > questNum && _player.QuestNum < 5)
            {
                string[] lines = ReadAllLines($"Content/Enemy/enemyLocations{_player.QuestNum}.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    int[] location = lines[i].Split(',').ToIntArray();
                    _enemies.Add(new Enemy(_content, location[0], location[1], enemyTypes[_player.QuestNum], _map.Tiles));
                }
                for (int i = 0; i < _enemies.Count; i++)
                {
                    Enemy enemy = _enemies[i];
                    foreach (Enemy enemy2 in _enemies)
                    {
                        enemy.OtherEnemies.Add(enemy2);
                    }
                    enemy.OtherEnemies.Remove(enemy);
                }
            }

            UpdateEnemies();

            _camera.Follow(_player.Position);

            UpdatePrevious();
        }

        private void UpdateEnemies()
        {
            if (_enemies.Count > 0)
            {
                for (int i = 0; i < _enemies.Count; i++)
                {
                    _enemies[i].Update((int)_player.Position.X, (int)_player.Position.Y);
                }
            }

            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].Dead) _enemies.Remove(_enemies[i]);
            }
        }
    }
}
