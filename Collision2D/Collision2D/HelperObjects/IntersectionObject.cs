using Collision2D.BasicGeometry;
using Collision2D.BoundingShapes;
using System;

namespace Collision2D.HelperObjects
{
    public enum IntersectionType
    {
        VerticalLine,
        HorizontalLine,
        Circle,
        Aabb,
        Wall
    }

    public class IntersectionObject: IComparable<IntersectionObject>
    {
        public bool Intersects { get; set; }
        public float Fraction { get; set; }
        public Point IntersectionPoint { get; set; }
        public Point ClosestPoint { get; set; }
        public float Distance { get; set; }
        public IntersectionType IntersectionType { get; set; }
        public Aabb Aabb { get; set; }

        public int CompareTo(IntersectionObject other)
        {
            return Fraction < other.Fraction ? -1 : 1;
        }
    }
}
