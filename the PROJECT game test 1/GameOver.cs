using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static Microsoft.Xna.Framework.Color;
using static Top_Down_Game.Game1;
namespace Top_Down_Game
{
    internal class GameOver : GameState
    {
        private List<Component> _components; // List of components, in this case buttons
        private Texture2D _texture;
        private Rectangle _rectangle;
        private Vector2 _position;
        public GameOver(ContentManager contentManager, GraphicsDevice graphicsDevice, Game1 _game, bool win) : base(contentManager, graphicsDevice, _game)
        { 
            if (win) _game._background = Gold; // If the player has won, the background will be gold
            else _game._background = IndianRed; // otherwise it will be red

            if (win) _texture = _content.Load<Texture2D>("Controls/Victory"); // If the player has won, display the victory image
            else _texture = _content.Load<Texture2D>("Controls/GameOver"); // otherwise display the death image

            _rectangle.X = 0;
            _rectangle.Y = 0;
            _rectangle.Width = _texture.Width;
            _rectangle.Height = _texture.Height;

            _position.X = WINDOW_WIDTH / 2 - (_texture.Width / 2); // Centers the image
            _position.Y = 100;

            var buttonTexture = _content.Load<Texture2D>("Controls/Button"); // The texture for the button
            var buttonFont = _content.Load<SpriteFont>("Fonts/font"); // The font for the buttons

            var mainMenu = new Button(buttonTexture, buttonFont) // The button that will start a new game when clicked
            {
                Position = new Vector2((WINDOW_WIDTH / 2) - buttonTexture.Width / 2, WINDOW_HEIGHT / 2),
                Text = "Return to Main Menu"
            };

            mainMenu.Click += MainMenuClick; // Adds a click event to the play button

            var quitButton = new Button(buttonTexture, buttonFont) // The button that will close the program when clicked
            {
                Position = new Vector2((WINDOW_WIDTH / 2) - buttonTexture.Width / 2, WINDOW_HEIGHT / 1.4f),
                Text = "Exit game"
            };

            quitButton.Click += QuitButtonClick; // Adds a click event to the quit button

            _components = new List<Component>()
            {
                mainMenu,
                quitButton
            };
        }
        private void QuitButtonClick(object sender, EventArgs e) => _game.Exit(); // Closes the program when the Exit button is clicked
        private void MainMenuClick(object sender, EventArgs e) => _game.ChangeState(new MainMenu(_content, _graphicsDevice, _game)); // Starts a new game
        public override void Update(GameTime gameTime)
        {
            foreach (var component in _components)
            {
                component.Update(gameTime); // Update each button
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (var component in _components)
            {
                component.Draw(spriteBatch); // Draw each button to the screen
            }

            spriteBatch.Draw(_texture, _position, _rectangle, White); // Draw the game over image at the top
        }
    }
}
