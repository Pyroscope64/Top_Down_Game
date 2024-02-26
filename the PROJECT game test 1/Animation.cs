using Microsoft.Xna.Framework;
using System.Collections.Generic;
using static System.Math;

namespace Top_Down_Game
{
    internal class Animation
    {
        private List<Rectangle> _sources;
        private bool _loop;
        public Animation(List<Rectangle> sources, bool loop)
        {
            _sources = sources;
            _loop = loop;
        }
        public Rectangle Source(ref int index)
        {
            int current = index;
            if (index == _sources.Count - 1) // If we're on the last frame
            {
                if (_loop) index = 0; // Looped animations return to frame 0
                else index = -1; // Otherwise, we mark as complete using -1
            }
            current = Clamp(current, 0, _sources.Count - 1);
            return _sources[current];
        }
        public static Animation Create(Point spriteSize, Point spriteSheetSize, int startIndex, int endIndex, bool loop)
        {
            int x, y;
            int columns = spriteSheetSize.X / spriteSize.X;
            List<Rectangle> sources = new List<Rectangle>();
            for (int i = startIndex; i <= endIndex; i++)
            {
                x = (i % columns) * spriteSize.X;
                y = (i / columns) * spriteSize.Y;
                sources.Add(new Rectangle(x, y, spriteSize.X, spriteSize.Y));
            }
            return new Animation(sources, loop);
        }
    }
}