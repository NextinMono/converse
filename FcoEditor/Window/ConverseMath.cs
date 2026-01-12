using System;
using System.Numerics;

namespace Converse
{
    public static class ConverseMath
    {


        public static Vector2 CalculatePivot(Vector2[] in_Quad)
        {
            // Find the min and max X and Y values from the 4 corners
            float minX = Math.Min(Math.Min(in_Quad[0].X, in_Quad[1].X), Math.Min(in_Quad[2].X, in_Quad[3].X));
            float maxX = Math.Max(Math.Max(in_Quad[0].X, in_Quad[1].X), Math.Max(in_Quad[2].X, in_Quad[3].X));

            float minY = Math.Min(Math.Min(in_Quad[0].Y, in_Quad[1].Y), Math.Min(in_Quad[2].Y, in_Quad[3].Y));
            float maxY = Math.Max(Math.Max(in_Quad[0].Y, in_Quad[1].Y), Math.Max(in_Quad[2].Y, in_Quad[3].Y));

            // Calculate the center of the quad (for simplicity as pivot example)
            Vector2 center = (in_Quad[0] + in_Quad[1] + in_Quad[2] + in_Quad[3]) / 4.0f;

            // Normalize the center based on the bounding box
            float normalizedX = (center.X - minX) / (maxX - minX);
            float normalizedY = (center.Y - minY) / (maxY - minY);

            return new Vector2(normalizedX, normalizedY);
        }
        public static int Gcd(int in_A, int in_B)
        {
            while (in_B != 0)
            {
                int temp = in_B;
                in_B = in_A % in_B;
                in_A = temp;
            }
            return in_A;
        }
        public static bool SameSide(Vector2 in_A, Vector2 in_B, Vector2 in_C, Vector2 in_P)
        {
            Vector2 ab = in_B - in_A;
            Vector2 ac = in_C - in_A;
            Vector2 ap = in_P - in_A;

            float cross1 = ab.X * ac.Y - ab.Y * ac.X;
            float cross2 = ab.X * ap.Y - ab.Y * ap.X;

            return cross1 * cross2 >= 0f;
        }

        public static Vector2 CenterOfRect(Vector2 in_TopLeft, Vector2 in_TopRight, Vector2 in_BtmRight, Vector2 in_BtmLeft)
        {
            return new Vector2(
                (in_TopLeft.X + in_BtmRight.X + in_TopRight.X + in_BtmLeft.X) / 4,
                (in_TopLeft.Y + in_BtmRight.Y + in_TopRight.Y + in_BtmLeft.Y) / 4
            );

        }
        /// <summary>
        /// Checks if a point is within a rect made up of 4 Vector2s.
        /// </summary>
        /// <param name="in_Point">Point to check</param>
        /// <param name="in_RTopLeft">Top left of rect</param>
        /// <param name="in_RTopRight">Top right of rect</param>
        /// <param name="in_RBtmRight">Bottom right of rect</param>
        /// <param name="in_RBtmLeft">Bottom left of rect</param>
        /// <returns></returns>
        public static bool IsPointInRect(Vector2 in_Point, Vector2 in_RTopLeft, Vector2 in_RTopRight, Vector2 in_RBtmRight, Vector2 in_RBtmLeft)
        {
            return SameSide(in_RTopLeft, in_RTopRight, in_RBtmRight, in_Point) && SameSide(in_RTopRight, in_RBtmRight, in_RBtmLeft, in_Point) &&
                   SameSide(in_RBtmRight, in_RBtmLeft, in_RTopLeft, in_Point) && SameSide(in_RBtmLeft, in_RTopLeft, in_RTopRight, in_Point);
        }
    }
}