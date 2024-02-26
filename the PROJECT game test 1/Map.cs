using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using static System.Convert;
using static Top_Down_Game.Tile;
using static Top_Down_Game.TileType;
using System.Runtime.CompilerServices;
using static System.IO.File;

namespace Top_Down_Game
{
    internal class Map
    {
        private ContentManager _content;
        public static int MAP_SIZE = 26;
        public List<Tile> Tiles { get; }
        public Map(ContentManager content)
        {
            _content = content;
            Tiles = new List<Tile>();
            GenerateMap();
        }
        private void GenerateMap()
        {
            bool isObstacle;
            string[] lines = ReadAllLines("Content/Obstacle/obstacle.txt");
            int halfWidth = MAP_SIZE * TILE_SIZE / 2;
            for (int y = 0; y < MAP_SIZE * TILE_SIZE; y += TILE_SIZE)
            {
                for (int x = 0; x < MAP_SIZE * TILE_SIZE; x += TILE_SIZE)
                {
                    isObstacle = false;
                    foreach (string line in lines)
                    {
                        string[] segments = line.Split(',');
                        if (x / TILE_SIZE == ToInt32(segments[1]) && y / TILE_SIZE == ToInt32(segments[2]))
                        {
                            Tiles.Add(new Tile(segments[0].ToEnum<TileType>(), _content, ToInt32(segments[1]) * TILE_SIZE, ToInt32(segments[2]) * TILE_SIZE, true));
                            isObstacle = true;
                        }
                    }

                    if (!isObstacle)
                    {
                        if (x < halfWidth && y < halfWidth) Tiles.Add(new Tile(Forest, _content, x, y, false));
                        else if (x >= halfWidth && y < halfWidth) Tiles.Add(new Tile(Mountain, _content, x, y, false));
                        else if (x < halfWidth && y >= halfWidth) Tiles.Add(new Tile(Wetland, _content, x, y, false));
                        else if (x >= halfWidth && y >= halfWidth) Tiles.Add(new Tile(Hotlands, _content, x, y, false));
                    }
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Tile tile in Tiles)
            {
                tile.Draw(spriteBatch);
            }
        }
    }
}