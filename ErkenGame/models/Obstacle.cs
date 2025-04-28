using Microsoft.Xna.Framework;

namespace ErkenGame.Models
{
    public class Obstacle
    {
        public Rectangle Rectangle { get; set; }

        public Obstacle(int x, int y, int width, int height)
        {
            Rectangle = new Rectangle(x, y, width, height);
        }
    }
}
