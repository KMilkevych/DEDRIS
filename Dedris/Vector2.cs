using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dedris
{
    public class Vector2
    {

        public double x { get; set; }
        public double y { get; set; }

        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double getLength()
        {
            double length = 0;

            length = Math.Sqrt(((this.x * this.x) + (this.y * this.y)));

            return length;
        }
    }
}
