using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.brg.Utilities
{
    public static class NumberUtilities
    {
        private static Dictionary<int, int> _powsOfTen;

        public static int LinearLerp(float ratio, int min, int max)
        {
            return (int)LinearLerp(ratio, (float)min, max);
        }

        public static float LinearLerp(float ratio, float min, float max)
        {
            return min + (max - min) * ratio;
        }

        public static float InverseLinearLerp(int value, int min, int max)
        {
            return InverseLinearLerp((float)value, min, max);
        }

        public static float InverseLinearLerp(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(min, value), max);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(min, value), max);
        }

        public static float Clamp01(float value)
        {
            return Clamp(value, 0.0f, 1.0f);
        }

                
        public static Vector2 RotateByRad(this Vector2 v, float radian) 
        {
            return new Vector2(
                (float)(v.x * Math.Cos(radian) - v.y * Math.Sin(radian)),
                (float)(v.x * Math.Sin(radian) + v.y * Math.Cos(radian))
            );
        }

        public static Vector2 RotateByDeg(this Vector2 v, float degree)
        {
            return v.RotateByRad(Mathf.Deg2Rad * degree);
        }

        public static float RadRotation(this Vector2 v)
        {
            return (float)Math.Atan2(v.y, v.x);
        }
        
        public static float DegRotation(this Vector2 v)
        {
            return v.RadRotation() * Mathf.Rad2Deg;
        }

        public static Color FromHex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}
