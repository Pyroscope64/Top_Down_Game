using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using static System.Convert;
using static System.IO.File;
using static Microsoft.Xna.Framework.Input.Keys;
using static Microsoft.Xna.Framework.Graphics.SpriteEffects;
using static Microsoft.Xna.Framework.Color;
using static Top_Down_Game.InputHandler;
using static Top_Down_Game.TileType;
using static Top_Down_Game.Game1;
using static Top_Down_Game.Map;
using static Top_Down_Game.Tile;
namespace Top_Down_Game
{
    internal class Player : Sprite
    {
        private const int KILL_OBJECTIVE = 15; // The number of enemies in each quest

        private Map _map; // All of the tiles on the map

        private ContentManager _content;
        private GraphicsDevice _graphics;
        private Game1 _game;
        private SpriteFont _font;

        private bool _displayQuestInfo; // Whether or not to display the quest info
        private int _displayQuestCounter; // The counter which increments as the quest info is being displayed
        private bool _questComplete; // Whether or not the player has completed the current quest
        public int QuestNum { get; set; } // The current quest that the player is completing

        private Texture2D _healthBar; // The texture of the health bar
        private Rectangle _healthBarRectangle; // The rectangle in which it will be displayed
        private Vector2 _healthBarPosition; // The position on the map

        private int _maxAttackCooldown; // The maximum length of time that the player has to wait to shoot a projectile
        private int _attackCooldown; // Current cooldown until the player can shoot a projectile again
        private int _maxHP; // Maximum Health Points that the player has
        public double HP { get; set; } // The player's current number of health points

        private const float SPRINT_MULTIPLIER = 1.5f; // The player's movement speed is multiplied by this when holding Shift
        private const float WATER_MULTIPLIER = 1.75f; // The player's movement speed is divided by this when in water


        private int _damage; // The player's current damage
        private float _movementSpeed; // The player's current movement speed 
        private float _maxMovementSpeed; // The maximum speed that the player can move at
        private double _regenSpeed; // The speed at which the player regenerates HP

        private bool _isInWater; // Whether or not the player is in water

        private NPC _npc; // The NPC

        private List<Projectile> _projectiles; // List of projectiles currently being shot by the player

        private List<Enemy> _enemies; // List of enemies currently in the game
        private bool _canShoot; // Whether or not the player is currently able to shoot a projectile
        public Vector2 Position => _position;  // Returns the position of the player, but does not set it to anything
        public Rectangle Collision => _collision; // The player's collision rectangle

        private int _kills; // The number of enemies that the player has defeated in the current quest
        public bool SpawnBoss { get; set; } // Whether or not the boss should spawn in
        public Player(Texture2D texture, ContentManager content, GraphicsDevice graphics, Vector2 position, 
            Dictionary<string, Animation> animations, Game1 game, Map map, List<Enemy> enemies) :
            base(texture, position, animations)
        {
            _map = map;

            _displayQuestInfo = false;
            _displayQuestCounter = 0;
            _questComplete = true; // Quest 0 starts completed, so that the first quest starts upon talking to the NPC

            _content = content;
            _graphics = graphics;

            int[] ints = ReadAllLines("Content/Player/player.txt").ToIntArray();
            _maxHP = ints[0]; // 200
            _damage = ints[1]; // 20
            _maxMovementSpeed = ints[2]; // 5
            _maxAttackCooldown = ints[3]; // 60
            _regenSpeed = (double)(ints[4] / 100m); // 0.05

            _attackCooldown = _maxAttackCooldown;
            HP = _maxHP / 2; // The player should start with full HP
            QuestNum = 0; // The player will not start with a quest

            _healthBar = content.Load<Texture2D>("Player/HealthBar");
            _healthBarRectangle.X = 0;
            _healthBarRectangle.Y = 0;
            _healthBarRectangle.Height = _healthBar.Height;

            _font = content.Load<SpriteFont>("Fonts/Font");

            _isInWater = false; // The player is not starting in water

            _game = game;

            _npc = new NPC(content);

            _projectiles = new List<Projectile>();

            _enemies = enemies;

            _canShoot = true;

            _kills = 0;
        }
        public override void Update()
        {
            CheckWinOrDie(); // Check if the player has either died or won
            if (_kills > 15) _kills = 15; // Prevents the game from glitching and stopping progression
            if (QuestNum < 5) IfInNPCRadius(); // Check if the player is within the NPC's radius
            IfInWater(); // Check if the player is currently touching a water tile
            GetInputs(); // Gets the current inputs
            UpdateProjectiles(); // Update each projectile that the player has currently shot

            if (HP < _maxHP) HP += _regenSpeed; // If the player is injured, heal
            if (HP > _maxHP) HP = _maxHP; // If the current regen speed would put the player above their max HP, reduce it to the maximum

            base.Update();

            UpdateHealthUI(); // The health UI should follow the player
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_displayQuestInfo && QuestNum < 6) DisplayQuestInfo(spriteBatch); // IF we should display the current quest info, do so
            if (QuestNum < 5) _npc.Draw(spriteBatch); // The NPC is only drawn before the 5th quest, which is the final boss
            spriteBatch.Draw(_texture, _position, _source, _colour, 0, _origin, _scale, _effects, 0); // Draws the player
            foreach (Projectile p in _projectiles) // Draws the current projectiles
            {
                p.Draw(spriteBatch);
            }
            spriteBatch.Draw(_healthBar, _healthBarPosition, _healthBarRectangle, White); // Draws the health bar
            spriteBatch.DrawString(_font, $"{Math.Floor(HP)}/{_maxHP}", _healthBarPosition, White); // Draws the current HP and the Max HP together
        }
        private void IfInWater()
        {
            _isInWater = false; // Assume the player is not in water
            foreach (Tile tile in _map.Tiles.Where(tile => tile.TileType == Wetland)) // If the player touches a water tile
            {
                if (_collision.Intersects(tile.Collision)) _isInWater = true; // They are in water
            }
        }
        private void IfInNPCRadius()
        {
            if (_collision.Intersects(_npc.Collision)) // If the player is inside the NPC's radius
            {
                if (KeyPressedOnce(E)) // And they are holding or pressing the E Key
                {
                    _npc.ShowDialogue = true; // The NPC should display dialogue
                    _npc.DisplayTooltip = false; // And not display the tooltip, as we want to see the dialogue

                    if (_kills == KILL_OBJECTIVE) // If we have defeated every enemy
                    {
                        _kills = 0; // Reset the kill count
                        _questComplete = true; // The quest is now complete
                        if (QuestNum < 5) GiveRewards(); // If we are not facing the final boss, give the player their rewards
                    }

                    if (_questComplete) // If they have just completed a quest
                    {
                        QuestNum++; // Increase the quest number upon talking to the NPC
                        _questComplete = false; // They have not completed this quest
                        _npc.DialogueIndex++; // The NPC will display the next set of dialogue
                    }

                    if (QuestNum == 5) // If we are on the final quest
                    {
                        SpawnBoss = true; // then the boss must be spawned
                    }

                }

                if (!_npc.ShowDialogue) _npc.DisplayTooltip = true; // If the NPC is not showing dialogue, they should display the tooltip
            }
            else // If the player is not in the NPC radius
            {
                _npc.DisplayTooltip = false; // The NPC should not display a tooltip
                _npc.ShowDialogue = false; // OR their dialogue
            }
        }
        private void GiveRewards()
        {
            int currentRewards = QuestNum; // Get the rewards for the current quest
            if (QuestNum > 4) currentRewards = 4; // If we have completed all of the quests, the current rewards will be the same (Prevents crashing)
            if (QuestNum == 0) currentRewards = 1; // If we have not started the game yet, the rewards will simply be the default (Prevents crashing)
            int[] allRewards = ReadAllLines($"Content/Quests/questReward{currentRewards}.txt").ToIntArray();
            _maxHP = allRewards[0];
            _damage = allRewards[1];
            _maxMovementSpeed = allRewards[2];
            _maxAttackCooldown = allRewards[3];
            _regenSpeed = (double)(allRewards[4] / 100m); // Divides the integer input to a double, rather than get a double input (Prevents crashing)
        }
        private void CheckWinOrDie()
        {
            if (HP <= 0) _game.ChangeState(new GameOver(_content, _graphics, _game, false)); // If the player has died, the game should show the death screen
            if (QuestNum >= 6) _game.ChangeState(new GameOver(_content, _graphics, _game, true)); // If the player has completed all of the quests, the game should show the victory screen
        }
        private void UpdateHealthUI()
        {
            _healthBarPosition.X = _position.X - (WINDOW_WIDTH - (_texture.Width / 2)) + 1270; // The health bar should be displayed in full at the far right
            _healthBarPosition.Y = _position.Y - (WINDOW_HEIGHT - (_texture.Height / 2)) + 220; // It should be displayed just below the top of the screen
            _healthBarRectangle.Width = ToInt32(_healthBar.Width * (ToDecimal(HP) / ToDecimal(_maxHP))); // The width of the bar should be proportionate to the actual HP divided by the maximum HP
        }
        private void DisplayQuestInfo(SpriteBatch spriteBatch)
        {
            Vector2 position; // The position to draw the quest information at
            Color colour;
            if (QuestNum < 5) colour = White; // UI detail
            else colour = Red;

            string[] lines = ReadAllLines($"Content/Quests/quest{QuestNum}.txt"); // Converts the quest file into a string array

            position.X = (int)_position.X - (WINDOW_WIDTH - (_texture.Width / 2)) + 320; // The position should be slightly in from the far left of the screen
            position.Y = (int)_position.Y - (WINDOW_HEIGHT - (_texture.Height / 2)) + 220; // The position should be slightly below the top of the screen

            for (int i = 1; i <= lines.Length; i++)
            {
                spriteBatch.DrawString(_font, lines[i - 1], new Vector2(position.X, position.Y + i * 30), colour); // Write each line with increasing y values of 30 for each line
            }

            if (QuestNum > 0 && QuestNum < 5) spriteBatch.DrawString(_font, $"{_kills}/15", new Vector2(position.X, position.Y + 150), Red);

            _displayQuestCounter++; // Increase the counter for how long the info will display for
        }
        private void UpdateProjectiles()
        {
            for (int i = 0; i < _projectiles.Count; i++)
            {
                _projectiles[i].Update(); // Update the current projectile
                if (_projectiles[i].KilledEnemy) _kills++; // If the player has defeated an enemy
                if (_projectiles[i].Dead) _projectiles.Remove(_projectiles[i]); // If the projectile has hit something, or travelled for too long, it disappears
            }
        }
        private void GetVelocity() // Arbitrary values of 34, 54, etc used as the sprite's collision values did not align with the sprite's drawn values
        {
            if (_position.X + _velocity.X < 34) _velocity.X = -_position.X + 34; // If the player is about to move out of the map, 
            else if (_position.X + _velocity.X > MAP_SIZE * TILE_SIZE) _velocity.X = MAP_SIZE * TILE_SIZE - _position.X; // reduce their velocity to the distance between them and the edge

            if (_position.Y + _velocity.Y < 54) _velocity.Y = -_position.Y + 54;
            else if (_position.Y + _velocity.Y > MAP_SIZE * TILE_SIZE) _velocity.Y = MAP_SIZE * TILE_SIZE - _position.Y;

            foreach (Tile tile in _map.Tiles.Where(tile => tile.Solid)) // The value + or - 15 prevents clipping to the other side of the tile
            {
                if (_position.X + _velocity.X < tile.Collision.X + TILE_SIZE + 34 && // If the player is about to collide with a solid tile
                    _position.X > tile.Collision.X + 15 && // and they are both within the top, right, and bottom bounds of the tile
                    _position.Y > tile.Collision.Y && _position.Y < tile.Collision.Y + TILE_SIZE)
                {
                    _velocity.X = -(_position.X - (tile.Collision.X + TILE_SIZE + 34)); // reduce their velocity to the distance between them and the left wall
                } // Repeat for every side of a solid tile

                if (_position.X + _velocity.X > tile.Collision.X &&
                    _position.X < tile.Collision.X + TILE_SIZE - 15 &&
                    _position.Y > tile.Collision.Y && _position.Y < tile.Collision.Y + TILE_SIZE)
                {
                    _velocity.X = tile.Collision.X - _position.X;
                }

                if (_position.Y + _velocity.Y < tile.Collision.Y + TILE_SIZE + 54 &&
                    _position.Y > tile.Collision.Y + 15 &&
                    _position.X > tile.Collision.X && _position.X < tile.Collision.X + TILE_SIZE)
                {
                    _velocity.Y = -(_position.Y - (tile.Collision.Y + TILE_SIZE + 54));
                }

                if (_position.Y + _velocity.Y > tile.Collision.Y &&
                    _position.Y < tile.Collision.Y + TILE_SIZE - 15 &&
                    _position.X > tile.Collision.X && _position.X < tile.Collision.X + TILE_SIZE)
                {
                    _velocity.Y = tile.Collision.Y - _position.Y;
                }
            }
        }
        private void GetInputs()
        {
            if (!_canShoot) _attackCooldown--; // If the player cannot shoot, reduce their current cooldown
            if (_attackCooldown <= 0) // If the cooldown has completed
            {
                _attackCooldown = _maxAttackCooldown; // set it back to the maximum
                _canShoot = true; // The player can now shoot a projectile again
            }

            _velocity = Vector2.Zero; // Set velocity to 0 so that it can be updated accordingly, instead of added to unnecessarily
            _busy = false; // There is no animation that is being played that cannot be interrupted, therefore we can start any action

            InputHandler.Update(); // Gets the current inputs
            if (MouseDown && _canShoot) // Shoots a projectile
            {
                _projectiles.Add(new Projectile(
                    _content,
                    _damage,
                    new List<Tile>(_map.Tiles.Where(tile => tile.Solid)),
                    _enemies,
                    _position,
                    MousePosition));
                _canShoot = false; // We cannot shoot a projectile now, as we have just shot one and must wait before shooting another
            }

            if (KeyDown(LeftShift)) _movementSpeed = _maxMovementSpeed * SPRINT_MULTIPLIER; // If sprinting, run faster
            else _movementSpeed = _maxMovementSpeed; // Else run at the regular speed

            if (_isInWater) _movementSpeed /= WATER_MULTIPLIER; // Divide the current movement speed by water multiplier to slow down in water

            if (KeyDown(Tab) && !_displayQuestInfo) _displayQuestInfo = true; // If Tab is being pressed and the quesst information is not currently being displayed, display it
            if (_displayQuestCounter == 300) // If we have displayed the quest info for 5 seconds...
            {
                _displayQuestInfo = false; // ...we should stop displaying it
                _displayQuestCounter = 0; // and reset the counter
            }

            if (KeyDown(Left) || KeyDown(A)) // If moving left or we are outside of the map
            {
                _effects = FlipHorizontally; // Face the other way if walking to the left (sprite facing left)...
                SetAction("Run"); // Displays the "run" animation
                _velocity.X = -_movementSpeed; // Velocity of X will be negative (moving left)
                _busy = true; // We are currently doing something therefore no other animation should play
            }
            if (KeyDown(Right) || KeyDown(D))
            {
                _effects = SpriteEffects.None; // ...else, face the right as according to the sprite
                SetAction("Run");
                _velocity.X = _movementSpeed;
                _busy = true;
            }
            if (KeyDown(Up) || KeyDown(W))
            {
                SetAction("Run");
                _velocity.Y = -_movementSpeed;
                _busy = true;
            }
            if (KeyDown(Down) || KeyDown(S))
            {
                SetAction("Run");
                _velocity.Y = _movementSpeed;
                _busy = true;
            }
            if (!_busy)
            {
                SetAction("Idle"); // We are not doing anything, so it should display the idle animation
                _busy = false; // We can start a new action
            }
            GetVelocity(); // Gets the current velocity (checks if there is a collision)
        }
    }
}