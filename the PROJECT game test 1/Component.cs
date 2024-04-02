using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Top_Down_Game
{
    public abstract class Component
    {
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void Update(GameTime gameTime);
    }
}