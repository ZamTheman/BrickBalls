using Collision2D.BasicGeometry;

namespace Collision2D.BoundingShapes
{
    public class Circle
    {
        public Point Center { get; set; }
        public float Radius { get; set; }
        public float Diameter { get => Radius * 2; }

        public Circle()
        {
            Center = new Point();
        }

        public Circle(Point center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
