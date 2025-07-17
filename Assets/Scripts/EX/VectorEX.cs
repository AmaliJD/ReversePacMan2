using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RandomOG = UnityEngine.Random;
using UnityEditor;

namespace EX
{
    public static class VectorEX
    {
        #region Constants
        // Constants
        public static readonly Vector2 upleft = new Vector2(-1, 1).normalized;
        public static readonly Vector2 upright = new Vector2(1, 1).normalized;
        public static readonly Vector2 downleft = new Vector2(-1, -1).normalized;
        public static readonly Vector2 downright = new Vector2(1, -1).normalized;
        #endregion

        #region Math
        // Vector Math
        public static bool Approximately(Vector2 v0, Vector2 v1, int decimals = -1)
        {
            if (decimals >= 0)
            {
                float power = Mathf.Pow(10, decimals);
                v0.x = (float)Mathf.Round(v0.x * power) / power;
                v0.y = (float)Mathf.Round(v0.y * power) / power;
                v1.x = (float)Mathf.Round(v1.x * power) / power;
                v1.y = (float)Mathf.Round(v1.y * power) / power;
            }
            return Mathf.Approximately(v0.x, v1.x) && Mathf.Approximately(v0.y, v1.y);
        }

        public static Vector3 Random(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
            => new Vector3(RandomOG.Range(xMin, xMax), RandomOG.Range(yMin, yMax), RandomOG.Range(zMin, zMax));

        public static Vector3 Random(float min, float max, bool includeNegativeRange = false)
        {
            float xRev, yRev, zRev;
            xRev = yRev = zRev = 1;

            if(includeNegativeRange)
            {
                xRev = RandomOG.Range(0, 2) == 0 ? -1 : 1;
                yRev = RandomOG.Range(0, 2) == 0 ? -1 : 1;
                zRev = RandomOG.Range(0, 2) == 0 ? -1 : 1;
            }

            return new Vector3(RandomOG.Range(xRev * min, xRev * max), RandomOG.Range(yRev * min, yRev * max), RandomOG.Range(zRev * min, zRev * max));
        }
        #endregion

        #region Lerps
        // Interpolation & Remapping
        public static Vector2 Lerp(Vector2 a, Vector2 b, Vector2 t) => new Vector2(Mathf.Lerp(a.x, b.x, t.x), Mathf.Lerp(a.y, b.y, t.y));
        public static Vector2 InverseLerp(Vector2 a, Vector2 b, Vector2 v) => (v - a) / (b - a);

        public static Vector2 Remap(Vector2 iMin, Vector2 iMax, Vector2 oMin, Vector2 oMax, Vector2 value)
        {
            Vector2 t = InverseLerp(iMin, iMax, value);
            return Lerp(oMin, oMax, t);
        }

        public static Vector2 Remap(Rect iRect, Rect oRect, Vector2 iPos)
        {
            return Remap(iRect.min, iRect.max, oRect.min, oRect.max, iPos);
        }

        public static Vector4 Remap(Vector4 origFrom, Vector4 origTo, Vector4 targetFrom, Vector4 targetTo, Vector4 value)
        {
            float rel = value.InverseLerp(origFrom, origTo);
            return Vector4.Lerp(targetFrom, targetTo, rel);
        }
        #endregion
    }
}
