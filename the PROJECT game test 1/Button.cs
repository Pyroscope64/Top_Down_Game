using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static Microsoft.Xna.Framework.Input.Mouse;
using static Microsoft.Xna.Framework.Input.ButtonState;
using static Microsoft.Xna.Framework.Color;
using System;

namespace Top_Down_Game
{
    public class Button : Component
    {
        private MouseState _currentMouse;
        private SpriteFont _spriteFont;
        private bool _isHovering;
        private MouseState _previousMouse;
        private Texture2D _texture;
        public event EventHandler Click;
        public bool Clicked { get; private set; }
        public Color PenColour { get; set; }
        public Vector2 Position { get; set; }
        public Rectangle Rectangle => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
        public string Text { get; set; }
        public Button(Texture2D texture, SpriteFont spriteFont)
        {
            _texture = texture;
            _spriteFont = spriteFont;
            PenColour = Black;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            var colour = White;
            if (_isHovering) colour = Gray;

            spriteBatch.Draw(_texture, Rectangle, colour);

            if (!string.IsNullOrEmpty(Text))
            {
                var x = Rectangle.X + (Rectangle.Width / 2) - (_spriteFont.MeasureString(Text).X / 2);
                var y = Rectangle.Y + (Rectangle.Height / 2) - (_spriteFont.MeasureString(Text).Y / 2);
                spriteBatch.DrawString(_spriteFont, Text, new Vector2(x, y), PenColour);
            }

        }

        public override void Update(GameTime gameTime)
        {
            _previousMouse = _currentMouse;
            _currentMouse = GetState();

            var mouseRectangle = new Rectangle(_currentMouse.X, _currentMouse.Y, 1, 1);

            _isHovering = false;

            if (mouseRectangle.Intersects(Rectangle))
            {
                _isHovering = true;

                if (_currentMouse.LeftButton == Released && _previousMouse.LeftButton == Pressed) 
                {
                    Click?.Invoke(this, new EventArgs());
                }
            }
        }
    }
}
