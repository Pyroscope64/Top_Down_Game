using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static System.IO.File;
using static System.Convert;
using System.Collections.Generic;

namespace Top_Down_Game
{
    internal class Boss : Enemy
    {
        public Boss(ContentManager content, List<Tile> tiles) : 
            base(content, (int)NPC.Position.X, (int)NPC.Position.Y, EnemyType.Boss, tiles)
        {
            _texture = content.Load<Texture2D>("");
            string[] lines = ReadAllLines("Content/Enemy/Boss.txt");
            foreach (string line in lines)
            {
                string[] segments = line.Split(',');
                _position.X = ToInt32(segments[0]);
                _position.Y = ToInt32(segments[1]);
                _hp = ToInt32(segments[2]);
                _damage = ToInt32(segments[3]);
                _movementSpeed = ToInt32(segments[4]);
                _type = segments[5].ToEnum<EnemyType>();
            }
        }
    }
}
