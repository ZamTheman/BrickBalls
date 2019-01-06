using Collision2D.BasicGeometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallBreaker.Debug
{
    public static class DebugData
    {
        private static Queue<Point> lastPositions = new Queue<Point>();
        public static void AddLastPosition(Point pnt)
        {
            lastPositions.Enqueue(new Point(pnt.X, pnt.Y));
            if (lastPositions.Count > 10)
                lastPositions.Dequeue();
        }

        public static Queue<Point> GetLastPositions()
        {
            return lastPositions;
        }

    }
}
