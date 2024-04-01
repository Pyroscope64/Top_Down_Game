namespace Top_Down_Game
{
    public class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int F { get; set; }
        public int G { get; set; }
        public int H { get; set; }
        public Node Parent { get; set; }
        public Node(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
