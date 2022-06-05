using System;

namespace EarthquakeWaring.App.Extensions;

public static class MathExtensions
{
    public static double ToRadians(this double val)
    {
        return (Math.PI / 180) * val;
    }
}