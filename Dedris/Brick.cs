using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dedris
{
    class Brick
    {
        public Vector2[] bricks { get; set; }
        public Vector2 baseCord { get; set; }
        public int type { get; set; }

        static int[,] figures;

        public Brick(int type, Vector2 basecord)
        {
            figures = new int[7, 4] 
            { 
                { 1, 3, 5, 7},
                {1, 2, 3, 5 },
                {0, 2, 4, 5 },
                {1, 3, 4, 5 },
                {0, 2, 3, 5 },
                {1, 2, 3, 4 },
                {0, 1, 2, 3}
            }; //I, T, L, J, S, Z, O

            this.type = type;
            this.baseCord = basecord;

            bricks = new Vector2[4];

            for (int i = 0; i < 4; i++)
            {
                bricks[i] = new Vector2(figures[type, i] % 2, (figures[type, i] - (figures[type, i] % 2))/2);
            }

        }
    }
}
