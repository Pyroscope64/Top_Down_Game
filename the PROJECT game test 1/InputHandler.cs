using Microsoft.Xna.Framework.Input;
using static Microsoft.Xna.Framework.Input.ButtonState;
using Microsoft.Xna.Framework;

namespace Top_Down_Game
{
    internal class InputHandler
    {
        private static KeyboardState _keyboardState; // Keys currently being pressed
        private static MouseState _mouseState; // Which cursor buttons are pressed + Cursor position
        private static MouseState _mouseStatePrevious; // Cursor buttons + position in the last frame
        public static Vector2 MousePosition => _mouseState.Position.ToVector2();  // Accessor for the mouse's position
        public static void Update()
        {
            _keyboardState = Keyboard.GetState(); // Gets the keyboard state for this frame
            _mouseState = Mouse.GetState(); // Gets the mouse state for this frame
        }
        public static void UpdatePrevious() => _mouseStatePrevious = _mouseState; // Gets the mouse state from the last frame
        public static bool KeyDown(Keys key) => _keyboardState.IsKeyDown(key); // Returns true if the key passed through is being pressed
        public static bool LMBClickedOnce => _mouseState.LeftButton == Pressed && _mouseStatePrevious.LeftButton == Released;
    }

}
