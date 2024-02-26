using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Top_Down_Game;

namespace Top_Down_Game
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameState _currentState; // Current state that the game is in
        private GameState _nextState; // The next state that the game will be in

        public const int WINDOW_WIDTH = 1500; // Width of the game window
        public const int WINDOW_HEIGHT = 900; // Height of the game window

        public Color _background; // A global variable to set the background of the screen in any game state
        public void ChangeState(GameState state) => _nextState = state; // ! Call this whenever the game state changes, e.g. Main Menu to InPlay
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
            Window.Title = "2D RPG Top Down Adventure Game";
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _currentState = new MainMenu(Content, GraphicsDevice, this);
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            if (_nextState != null)
            {
                _currentState = _nextState;
                _nextState = null;
            }

            _currentState.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_background);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            _currentState.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}