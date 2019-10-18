using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dedris
{
    public static class VectorFunctions
    {

        public static Vector2 Sum(Vector2 vectorA, Vector2 vectorB)
        {
            Vector2 sum = new Vector2(vectorA.x + vectorB.x, vectorA.y + vectorB.y);
            return sum;
        }

        public static Vector2 Difference(Vector2 vectorA, Vector2 vectorB)
        {
            Vector2 difference = new Vector2(vectorA.x - vectorB.x, vectorA.y - vectorB.y);
            return difference;
        }

        public static Vector2 Product(Vector2 vectorA, double scalar)
        {
            Vector2 product = new Vector2(vectorA.x * scalar, vectorA.y * scalar);
            return product;
        }

        public static double DotProduct(Vector2 vectorA, Vector2 vectorB)
        {
            double dotProduct = vectorA.x * vectorB.x + vectorA.y * vectorB.y;
            return dotProduct;
        }

        public static double Length(Vector2 vectorA)
        {
            double length = Math.Sqrt(Math.Pow(vectorA.x, 2) + Math.Pow(vectorA.y, 2));
            return length;
        }

        public static double Angle(Vector2 vectorA, Vector2 vectorB)
        {
            double angle = 0;
            double product = DotProduct(vectorA, vectorB);
            double lengthproduct = Length(vectorA) * Length(vectorB);
            double fraction = product / lengthproduct;
            double radians = Math.Acos(fraction);
            angle = (radians * 180) / Math.PI;


            return angle;
        }

        public static Vector2 Orthogonal(Vector2 vectorA)
        {
            Vector2 orthogonal = new Vector2((-1)*vectorA.y, vectorA.x);
            return orthogonal;
        }

        public static Vector2 Projection(Vector2 vectorA, Vector2 vectorB)
        {
            Vector2 projection = new Vector2(0, 0);

            double product = DotProduct(vectorA, vectorB);
            double secondproduct = DotProduct(vectorB, vectorB);
            double fraction = product / secondproduct;
            projection = Product(vectorB, fraction);

            return projection;
        }

        public static double Determinant(Vector2 vectorA, Vector2 vectorB)
        {
            double determinant = DotProduct(Orthogonal(vectorA), vectorB);
            return determinant;
        }

        public static double TriangleArea(Vector2 vectorA, Vector2 vectorB)
        {
            double determinant = Determinant(vectorA, vectorB);
            if (determinant < 0)
            {
                determinant = determinant * (-1);
            }
            double area = determinant/2;
            return area;

        }

        public static double ParallelogramArea(Vector2 vectorA, Vector2 vectorB)
        {
            double determinant = Determinant(vectorA, vectorB);
            if (determinant < 0)
            {
                determinant = determinant * (-1);
            }
            return determinant;
        }
    }
}
