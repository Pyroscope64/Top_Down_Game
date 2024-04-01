using Microsoft.Xna.Framework.Input;
using static Microsoft.Xna.Framework.Input.ButtonState;
using Microsoft.Xna.Framework;

namespace Top_Down_Game
{
    public static class InputHandler
    {
        private static KeyboardState _keyboardState; // Keys currently being pressed
        private static KeyboardState _keyboardStatePrevious;
        private static MouseState _mouseState; // Which cursor buttons are pressed + Cursor position
        public static Vector2 MousePosition => _mouseState.Position.ToVector2();  // Accessor for the mouse's position
        public static bool MouseDown => _mouseState.LeftButton == Pressed;
        public static bool KeyDown(Keys key) => _keyboardState.IsKeyDown(key); // Returns true if the key passed through is being pressed
        public static bool KeyPressedOnce(Keys key) => _keyboardState.IsKeyDown(key) && !_keyboardStatePrevious.IsKeyDown(key);
        public static void Update()
        {
            _keyboardState = Keyboard.GetState(); // Gets the keyboard state for this frame
            _mouseState = Mouse.GetState(); // Gets the mouse state for this frame
        }
        public static void UpdatePrevious()
        {
            _keyboardStatePrevious = _keyboardState;
        } // Gets the mouse state from the last frame
    }

}
