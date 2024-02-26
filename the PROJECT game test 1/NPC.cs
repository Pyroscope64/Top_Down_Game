using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using static Microsoft.Xna.Framework.Color;
using static System.IO.File;

namespace Top_Down_Game
{
    internal class NPC
    {
        private SpriteFont _font;
        private Texture2D _texture;
        private Rectangle _rectangle;
        private Rectangle _collision;
        private Vector2 _dialoguePosition;
        public static Vector2 Position { get; private set; }
        public Rectangle Collision => _collision;
        public bool DisplayTooltip { get; set; }
        public bool ShowDialogue { get; set; }
        public int DialogueIndex { get; set; }
        public NPC(ContentManager content)
        {
            _font = content.Load<SpriteFont>("Fonts/Font");
            _texture = content.Load<Texture2D>("NPC/NPC"); 

            Position = new Vector2(500, 500); 

            _collision.X = (int)(Position.X - (_texture.Width * 2)); 
            _collision.Y = (int)(Position.Y - (_texture.Height * 2));
            _collision.Width = (int)(_collision.X + _collision.X - Position.X);
            _collision.Height = (int)(_collision.Y + _collision.Y - Position.Y);

            _rectangle.Width = _texture.Width;
            _rectangle.Height = _texture.Height;
            _rectangle.X = 0;
            _rectangle.Y = 0;

            ShowDialogue = false;
            DialogueIndex = 0;
            _dialoguePosition = new Vector2(Position.X, Position.Y + _texture.Height * 2);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, _rectangle, White);
            if (DisplayTooltip) spriteBatch.DrawString(_font, "Press E to talk to the NPC", _dialoguePosition, White);
            if (ShowDialogue) DisplayDialogue(spriteBatch, DialogueIndex);
        }
        private void DisplayDialogue(SpriteBatch spriteBatch, int dialogueNum)
        {
            string[] lines = ReadAllLines($"Content/NPC/Dialogue/dialogue{dialogueNum}.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                spriteBatch.DrawString(_font, lines[i], new Vector2(_dialoguePosition.X, _dialoguePosition.Y + i * 30), White);
            }
        }
    }
}
