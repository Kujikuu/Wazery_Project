using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project
{
    public partial class MyMath
    {
        private static SafeRandom Rand = new SafeRandom(Environment.TickCount);

        public const Int32 NORMAL_RANGE = 17;
        public const Int32 BIG_RANGE = 34;
        public const Int32 USERDROP_RANGE = 9;
        /// <summary>
        /// Generate a number in a specified range. (Number ∈ [Min, Max])
        /// </summary>
        public static Int32 Generate(Int32 Min, Int32 Max)
        {
            if (Max != Int32.MaxValue)
                Max++;

            Int32 Value = 0;
            /*lock (Rand) { */
            Value = Rand.Next(Min, Max); /*}*/
            return Value;
        }
        public static double PointDirecton(double x1, double y1, double x2, double y2)
        {
            double direction = 0;

            double AddX = x2 - x1;
            double AddY = y2 - y1;
            double r = (double)Math.Atan2(AddY, AddX);

            if (r < 0) r += (double)Math.PI * 2;

            direction = 360 - (r * 180 / (double)Math.PI);
            return direction;
        }
        public static Boolean Success(Double Chance) { return ((Double)Generate(1, 1000000)) / 10000 >= 100 - Chance; }
    }
}
