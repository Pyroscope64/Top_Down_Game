using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static Top_Down_Game.Game1;
using static Microsoft.Xna.Framework.Color;

namespace Top_Down_Game
{
    public class MainMenu : GameState
    {
        private List<Component> _components; // List of components, in this case the button
        private Texture2D _texture; 
        private Rectangle _rectangle; 
        private Vector2 _position;

        private Camera _camera;
        public MainMenu(ContentManager contentManager, GraphicsDevice graphicsDevice, Game1 _game) : base(contentManager, graphicsDevice, _game)
        {
            _game._background = DarkOliveGreen; // The background colour of the game, to be replaced with a tile

            _texture = _content.Load<Texture2D>("Controls/MainMenu"); // Loads the button texture from the file

            _rectangle.X = 0; 
            _rectangle.Y = 0;
            _rectangle.Width = _texture.Width;
            _rectangle.Height = _texture.Height;

            _position.X = WINDOW_WIDTH / 2 - (_texture.Width / 2); // Perfectly centred on the X axis
            _position.Y = 100; // Slightly below the top of the screen

            var buttonTexture = _content.Load<Texture2D>("Controls/Button"); // The texture for the button
            var buttonFont = _content.Load<SpriteFont>("Fonts/font"); // The font for the buttons

            var playButton = new Button(buttonTexture, buttonFont) // The button that will start a new game when clicked
            {
                Position = new Vector2((WINDOW_WIDTH / 2) - buttonTexture.Width / 2, WINDOW_HEIGHT / 2),
                Text = "Start new game"
            };

            playButton.Click += PlayButtonClick; // Adds a click event to the play button

            var quitButton = new Button(buttonTexture, buttonFont) // The button that will close the program when clicked
            {
                Position = new Vector2((WINDOW_WIDTH / 2) - buttonTexture.Width / 2, WINDOW_HEIGHT / 1.4f),
                Text = "Exit game"
            };

            quitButton.Click += QuitButtonClick; // Adds a click event to the quit button

            _components = new List<Component>()
            {
                playButton,
                quitButton
            };

            _camera = new Camera(new Point(WINDOW_WIDTH, WINDOW_HEIGHT));
        }

        private void QuitButtonClick(object sender, EventArgs e) => _game.Exit(); // Closes the program when the Exit button is clicked

        private void PlayButtonClick(object sender, EventArgs e) => _game.ChangeState(new InPlay(_content, _graphicsDevice, _game, _camera)); // Starts a new game

        public override void Update(GameTime gameTime)
        {
            foreach (var component in _components)
            {
                component.Update(gameTime); // Update for each button
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach ( var component in _components)
            {
                component.Draw(spriteBatch); // Draw each button
            }

            spriteBatch.Draw(_texture, _position, _rectangle, White);
        }
    }
}