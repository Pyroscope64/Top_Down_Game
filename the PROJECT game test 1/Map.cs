using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static System.Convert;
using static System.IO.File;
using static Top_Down_Game.Tile;
using static Top_Down_Game.TileType;
namespace Top_Down_Game
{
    internal class Map
    {
        private ContentManager _content;
        public static int MAP_SIZE = 26; // The map size is 26, meaning there are 26 by 26 tiles
        public List<Tile> Tiles { get; } // The list of all tiles
        public Map(ContentManager content)
        {
            _content = content;
            Tiles = new List<Tile>();
            GenerateMap();
        }
        private void GenerateMap()
        {
            bool isObstacle; // Whether or not the current tile is an obstacle
            string[] lines = ReadAllLines("Content/Obstacle/obstacle.txt"); // All of the obstacles
            int halfWidth = MAP_SIZE * TILE_SIZE / 2; // Used to split the map in 4
            for (int y = 0; y < MAP_SIZE * TILE_SIZE; y += TILE_SIZE)
            {
                for (int x = 0; x < MAP_SIZE * TILE_SIZE; x += TILE_SIZE)
                {
                    isObstacle = false; // Assume the tile is not an obstacle
                    foreach (string line in lines)
                    {
                        string[] segments = line.Split(',');
                        if (x / TILE_SIZE == ToInt32(segments[1]) && y / TILE_SIZE == ToInt32(segments[2])) // If it is an obstacle
                        {
                            Tiles.Add(new Tile(segments[0].ToEnum<TileType>(), _content, ToInt32(segments[1]) * TILE_SIZE, ToInt32(segments[2]) * TILE_SIZE, true));
                            isObstacle = true; // then we create the tile as sa solid tile
                        }
                    }

                    if (!isObstacle) // If it is not an obstacle, then we create the tile, giving it the type depending on its position.
                    {
                        if (x < halfWidth && y < halfWidth) Tiles.Add(new Tile(Forest, _content, x, y, false)); // Top left = Forest
                        else if (x >= halfWidth && y < halfWidth) Tiles.Add(new Tile(Mountain, _content, x, y, false)); // Top right = Mountain
                        else if (x < halfWidth && y >= halfWidth) Tiles.Add(new Tile(Wetland, _content, x, y, false)); // Bottom left = Wetland
                        else if (x >= halfWidth && y >= halfWidth) Tiles.Add(new Tile(Hotlands, _content, x, y, false)); // Bottom right = Hotlands
                    }
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Tile tile in Tiles)
            {
                tile.Draw(spriteBatch); // Draws each tile
            }
        }
    }
}