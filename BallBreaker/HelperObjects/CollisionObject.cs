using BallBreaker.Sprites;
using Collision2D.HelperObjects;
using System;

namespace BallBreaker.HelperObjects
{
    public enum Wall
    {
        Left,
        Right,
        Top
    }

    public class CollisionObject: IComparable<CollisionObject>
    {
        public IntersectionObject IntersectionObject { get; set; }
        public Brick Brick { get; set; }
        public Wall Wall { get; set; }

        public int CompareTo(CollisionObject other)
        {
            return IntersectionObject.Fraction < other.IntersectionObject.Fraction ? -1 : 1;
        }
    }
}
