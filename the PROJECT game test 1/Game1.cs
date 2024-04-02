using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            base.Initialize();
            Window.Title = "2D RPG Top Down Adventure Game"; // Sets the title 
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH; // Sets the resolution
            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _currentState = new MainMenu(Content, GraphicsDevice, this); // Start the game in the main menu
            _graphics.ApplyChanges();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        protected override void Update(GameTime gameTime)
        {
            if (_nextState != null) // If we have a state to change to
            {
                _currentState = _nextState; // change to that state
                _nextState = null; // and assume that we do not need to change state yet
            }

            _currentState.Update(gameTime); // Update the current state

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_background);

            _spriteBatch.Begin();

            _currentState.Draw(_spriteBatch); // Draw the current state to the screen

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}