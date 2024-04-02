namespace Top_Down_Game
{
    public class Node
    {
        public int X { get; set; } // X position
        public int Y { get; set; } // Y position
        public int F { get; set; } // Cost of this node (G + H)
        public int G { get; set; } // Distance that this node has travelled from the start node
        public int H { get; set; } // The heuristic distance between this node and the player
        public Node Parent { get; set; } // The parent node for this node
        public Node(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
