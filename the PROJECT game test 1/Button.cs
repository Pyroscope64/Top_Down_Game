using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static Microsoft.Xna.Framework.Input.Mouse;
using static Microsoft.Xna.Framework.Input.ButtonState;
using static Microsoft.Xna.Framework.Color;
namespace Top_Down_Game
{
    public class Button : Component
    {
        private MouseState _currentMouse; // The current mouse state (buttons being pressed, position, etc)
        private SpriteFont _spriteFont;
        private bool _isHovering; // Whether or not the mouse is currently on a button
        private MouseState _previousMouse; // The mouse state from the previous frame
        private Texture2D _texture;
        public event EventHandler Click; // What to do upon clicking a button
        public bool Clicked { get; private set; } // Whether or not a button has been clicked
        public Color PenColour { get; set; } 
        public Vector2 Position { get; set; }
        public Rectangle Rectangle => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
        public string Text { get; set; } // The text being displayed in the button
        public Button(Texture2D texture, SpriteFont spriteFont)
        {
            _texture = texture;
            _spriteFont = spriteFont;
            PenColour = Black;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            var colour = White;
            if (_isHovering) colour = Gray; // We dim the button to indicate that it is being hovered over

            spriteBatch.Draw(_texture, Rectangle, colour);

            if (!string.IsNullOrEmpty(Text)) // Draws the text in the center of the string (if there is text)
            {
                var x = Rectangle.X + (Rectangle.Width / 2) - (_spriteFont.MeasureString(Text).X / 2);
                var y = Rectangle.Y + (Rectangle.Height / 2) - (_spriteFont.MeasureString(Text).Y / 2);
                spriteBatch.DrawString(_spriteFont, Text, new Vector2(x, y), PenColour);
            }
        }
        public override void Update(GameTime gameTime)
        {
            _previousMouse = _currentMouse; // Set the previous state to the current state, before updating the current state
            _currentMouse = GetState(); // Gets the current state of the mouse

            var mouseRectangle = new Rectangle(_currentMouse.X, _currentMouse.Y, 1, 1);

            _isHovering = false; // Assume we are not hovering over a button

            if (mouseRectangle.Intersects(Rectangle)) // If the mouse is touching a button
            {
                _isHovering = true; // We are hovering

                if (_currentMouse.LeftButton == Released && _previousMouse.LeftButton == Pressed) // If we click on the button in this frame but not the last 
                {
                    Click?.Invoke(this, new EventArgs()); // Then we have clicked on the button
                }
            }
        }
    }
}
