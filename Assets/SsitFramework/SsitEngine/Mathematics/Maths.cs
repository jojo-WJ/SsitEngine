namespace SsitEngine.Mathematics
{
    /// <summary>Mathematical concepts and methods.</summary>
    public static class Maths
    {
        /// <summary>Interpolates between a and b by t.</summary>
        /// <param name="from">Start value of interpolate value.</param>
        /// <param name="to">End value of interpolate value.</param>
        /// <param name="t">t is clamped between 0 and 1.</param>
        /// <returns></returns>
        public static double Lerp( double from, double to, double t )
        {
            return from + (to - from) * t;
        }
    }
}