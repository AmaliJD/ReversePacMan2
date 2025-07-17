// collected and expended upon by Freya Holmér (https://github.com/FreyaHolmer/Mathfs)

using Unity.VisualScripting;
using UnityEngine;
using static EX.MathEX;
using static UnityEngine.Rendering.DebugUI;

namespace EX
{

    public static class MathExtensions {

        public static float Frac( this float v ) => v - MathEX.Floor( v );
		public static float Clamp01( this float v ) => MathEX.Clamp01( v );
		public static float ClampNeg1to1( this float v ) => MathEX.ClampNeg1to1( v );
		

		public static float Round( this float v, float snapInterval ) => MathEX.Round( v, snapInterval );
		public static bool Within( this float v, float min, float max ) => v >= min && v <= max;
        public static bool NotWithin(this float v, float min, float max) => v <= min || v >= max;
        public static bool Between( this float v, float min, float max ) => v > min && v < max;
        public static bool NotBetween(this float v, float min, float max) => v < min || v > max;
        public static float AtLeast( this float v, float min ) => MathEX.Max( v, min );
		public static float AtMost( this float v, float max ) => MathEX.Min( v, max );

		public static float Square( this float v ) => v * v;
		public static float Abs( this float v ) => MathEX.Abs( v );
        public static bool SignedGreaterThan(this float v, float value)
        {
            if (value > 0)
                return v > value;
            else if (value < 0)
                return v < value;
            else
                return v != value;
        }
        public static bool SignedLessThan(this float v, float value)
        {
            if (value > 0)
                return v < value;
            else if (value < 0)
                return v > value;
            else
                return v != value;
        }

        public static float AddRelativeToSign(this float v, float add)
        {
            if (v >= 0)
                return v + add;
            else
                return v + -add;
        }

        public static float Sign(this float v) => MathEX.Sign(v);
        public static float SignWithZero(this float v) => MathEX.SignWithZero(v);
        public static float SignWithZero(this float v, float epsilon = 0.000001f) => MathEX.SignWithZero(v, epsilon);

        public static float SignSetZero(this float v, bool positive) => v == 0 ? (positive ? 1 : -1) : MathEX.Sign(v);
        public static float SignSetZero(this float v, float zeroValue) => v == 0 ? zeroValue : MathEX.Sign(v);
        public static float SignSetMultipliers(this float v, float positiveScale, float negativeScale) => MathEX.SignWithZero(v) * (MathEX.SignWithZero(v) == 1 ? positiveScale : negativeScale);
        public static float Magnitude( this float v ) => MathEX.Abs( v );
		public static float Clamp( this float v, float min, float max ) => MathEX.Clamp( v, min, max );
		public static float Remap( this float v, float iMin, float iMax, float oMin, float oMax ) => MathEX.Remap( iMin, iMax, oMin, oMax, v );
		public static float Repeat( this float v, float length ) => MathEX.Repeat( v, length );
		public static int Mod( this int value, int length ) => ( value % length + length ) % length; // modulo
		public static int NextMod( this int value, int length ) => Mod( value + 1, length );
		public static int PrevMod( this int value, int length ) => Mod( value - 1, length );
		public static int RoundToInt( this float v ) => MathEX.RoundToInt( v );
		public static int FloorToInt( this float v ) => MathEX.FloorToInt( v );
		public static int CeilToInt( this float v ) => MathEX.CeilToInt( v );

        public static float PerFrame(this float f) => f * Time.deltaTime;
        public static float PerFixedFrame(this float f) => f * Time.fixedDeltaTime;
        public static float Scale(this float f, float value) => f * value;
    }

    public static class VectorExtensions
	{
        public static float Angle(this Vector2 v) => Mathf.Atan2(v.y, v.x);

        public static Vector2 Rotate90CW(this Vector2 v) => new Vector2(v.y, -v.x);
        public static Vector2 Rotate90CCW(this Vector2 v) => new Vector2(-v.y, v.x);

        public static Vector2 RotateAround(this Vector2 v, Vector2 pivot, float angle, AngleUnits units = AngleUnits.Degrees) => Rotate(v - pivot, AngleUnitConversion(angle, units, AngleUnits.Radians)) + pivot;

        public static Vector2 Rotate(this Vector2 v, float angle, AngleUnits units = AngleUnits.Degrees)
        {
            if (angle == 0)
                return v;

            angle = AngleUnitConversion(angle, units, AngleUnits.Radians);

            float ca = Mathf.Cos(angle);
            float sa = Mathf.Sin(angle);
            return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
        }

        public static Vector2 RotateRange(this Vector2 v, float angleRange, AngleUnits units = AngleUnits.Degrees)
        {
            angleRange = AngleUnitConversion(angleRange, units, AngleUnits.Radians);

            float randomAngle = Random.Range(-angleRange, angleRange);
            return v.Rotate(randomAngle);
        }

        public static Vector2 RotateRangeExclude(this Vector2 v, float angleRange, float angleRangeExcluded, AngleUnits units = AngleUnits.Degrees)
        {
            angleRange = AngleUnitConversion(angleRange, units, AngleUnits.Radians);
            angleRangeExcluded = AngleUnitConversion(angleRangeExcluded, units, AngleUnits.Radians);

            int rand = UnityEngine.Random.Range(0, 2);
            float randomAngle = rand == 0 ? Random.Range(angleRangeExcluded, angleRange) : Random.Range(-angleRange, -angleRangeExcluded);
            return v.Rotate(randomAngle);
        }

        public static float InverseLerp(this Vector2 value, Vector2 a, Vector2 b)
        {
            Vector2 AB = b - a;
            Vector2 AV = value - a;
            return Vector2.Dot(AV, AB) / Vector2.Dot(AB, AB);
        }
        public static float InverseLerp(this Vector3 value, Vector3 a, Vector3 b)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }
        public static float InverseLerp(this Vector4 value, Vector4 a, Vector4 b)
        {
            Vector4 AB = b - a;
            Vector4 AV = value - a;
            return Vector4.Dot(AV, AB) / Vector4.Dot(AB, AB);
        }

        public static Vector2 XZ(this Vector3 v) => new Vector2(v.x, v.z);
        public static Vector2 XY(this Vector3 v) => new Vector2(v.x, v.y);
        public static Vector2 YZ(this Vector3 v) => new Vector2(v.y, v.z);
        public static Vector3 XZtoXYZ(this Vector2 v, float y = 0) => new Vector3(v.x, y, v.y);

        public static Vector2 Frac(this Vector2 v) => v - v.Floor();
        public static Vector3 Frac(this Vector3 v) => v - v.Floor();
        public static Vector4 Frac(this Vector4 v) => v - v.Floor();

        public static Vector2 Clamp01(this Vector2 v) => new Vector2(MathEX.Clamp01(v.x), MathEX.Clamp01(v.y));
        public static Vector3 Clamp01(this Vector3 v) => new Vector3(MathEX.Clamp01(v.x), MathEX.Clamp01(v.y), MathEX.Clamp01(v.z));
        public static Vector4 Clamp01(this Vector4 v) => new Vector4(MathEX.Clamp01(v.x), MathEX.Clamp01(v.y), MathEX.Clamp01(v.z), MathEX.Clamp01(v.w));

        public static float Min(this Vector2 v) => MathEX.Min(v.x, v.y);
        public static float Min(this Vector3 v) => MathEX.Min(v.x, v.y, v.z);
        public static float Min(this Vector4 v) => MathEX.Min(v.x, v.y, v.z, v.w);
        public static float Max(this Vector2 v) => MathEX.Max(v.x, v.y);
        public static float Max(this Vector3 v) => MathEX.Max(v.x, v.y, v.z);
        public static float Max(this Vector4 v) => MathEX.Max(v.x, v.y, v.z, v.w);

        public static Vector2 Abs(this Vector2 vector) => new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        public static Vector2 SetSign(this Vector2 vector, bool positiveX, bool positiveY) => new Vector2(Mathf.Abs(vector.x) * (positiveX ? 1 : -1), Mathf.Abs(vector.y) * (positiveY ? 1 : -1));

        public static Vector2 WithMagnitude(this Vector2 v, float mag) => v.normalized * mag;
        public static Vector3 WithMagnitude(this Vector3 v, float mag) => v.normalized * mag;

        public static Vector2 SignWithZero(this Vector2 v, float epsilon = 0.000001f) => new Vector2(v.x.SignWithZero(epsilon), v.y.SignWithZero(epsilon));
        public static Vector2 ZeroNegatives(this Vector2 v) => new Vector2(v.x > 0 ? v.x : 0, v.y > 0 ? v.y : 0);

        public static Vector2 ClampMagnitude(this Vector2 v, float min, float max)
        {
            float mag = v.magnitude;
            if (mag < min)
            {
                Vector2 dir = v / mag;
                return dir * min;
            }

            if (mag > max)
            {
                Vector2 dir = v / mag;
                return dir * max;
            }

            return v;
        }

        public static Vector3 ClampMagnitude(this Vector3 v, float min, float max)
        {
            float mag = v.magnitude;
            if (mag < min)
            {
                Vector3 dir = v / mag;
                return dir * min;
            }

            if (mag > max)
            {
                Vector3 dir = v / mag;
                return dir * max;
            }

            return v;
        }

        public static Vector2 ZeroMagnitudeCutoff(this Vector2 v, float cutoff) => v.magnitude < cutoff ? Vector2.zero : v;

        public static Vector3 FlattenY(this Vector3 v) => new Vector3(v.x, 0f, v.z);

        public static Vector2 To(this Vector2 v, Vector2 target) => target - v;
        public static Vector3 To(this Vector3 v, Vector3 target) => target - v;
        public static Vector2 DirTo(this Vector2 v, Vector2 target) => (target - v).normalized;
        public static Vector3 DirTo(this Vector3 v, Vector3 target) => (target - v).normalized;

        public static Vector2 Truncate(this Vector2 v) => new Vector2(MathEX.Truncate(v.x), MathEX.Truncate(v.y));
        public static Vector2 Floor(this Vector2 v) => new Vector2(MathEX.Floor(v.x), MathEX.Floor(v.y));
        public static Vector3 Floor(this Vector3 v) => new Vector3(MathEX.Floor(v.x), MathEX.Floor(v.y), MathEX.Floor(v.z));
        public static Vector4 Floor(this Vector4 v) => new Vector4(MathEX.Floor(v.x), MathEX.Floor(v.y), MathEX.Floor(v.z), MathEX.Floor(v.w));
        public static Vector2 Ceil(this Vector2 v) => new Vector2(MathEX.Ceil(v.x), MathEX.Ceil(v.y));
        public static Vector3 Ceil(this Vector3 v) => new Vector3(MathEX.Ceil(v.x), MathEX.Ceil(v.y), MathEX.Ceil(v.z));
        public static Vector4 Ceil(this Vector4 v) => new Vector4(MathEX.Ceil(v.x), MathEX.Ceil(v.y), MathEX.Ceil(v.z), MathEX.Ceil(v.w));
        public static Vector2 Round(this Vector2 v) => new Vector2(MathEX.Round(v.x), MathEX.Round(v.y));
        public static Vector2Int RoundToInt(this Vector2 v) => new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        public static Vector3 Round(this Vector3 v) => new Vector3(MathEX.Round(v.x), MathEX.Round(v.y), MathEX.Round(v.z));
        public static Vector3Int RoundToInt(this Vector3 v) => new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        public static Vector4 Round(this Vector4 v) => new Vector4(MathEX.Round(v.x), MathEX.Round(v.y), MathEX.Round(v.z), MathEX.Round(v.w));

        public static Vector3 Add(this Vector3 vector, float x, float y) { vector.x += x; vector.y += y; return vector; }

        public static Vector2 Set(this Vector2 vector, float x, float y) { vector.x = x; vector.y = y; return vector; }
        public static Vector2 SetX(this Vector2 vector, float x) { vector.x = x; return vector; }
        public static Vector2 SetY(this Vector2 vector, float y) { vector.y = y; return vector; }
        public static Vector2 SetRef(ref this Vector2 vector, float x, float y) { vector.x = x; vector.y = y; return vector; }
        public static Vector2 SetXRef(ref this Vector2 vector, float x) { vector.x = x; return vector; }
        public static Vector2 SetYRef(ref this Vector2 vector, float y) { vector.y = y; return vector; }
        public static Vector3 Set(this Vector3 vector, float x, float y, float z) { vector.x = x; vector.y = y; vector.z = z; return vector; }
        public static Vector3 SetXY(this Vector3 vector, float x, float y) { vector.x = x; vector.y = y; return vector; }
        public static Vector3 SetXZ(this Vector3 vector, float x, float z) { vector.x = x; vector.z = z; return vector; }
        public static Vector3 SetYZ(this Vector3 vector, float y, float z) { vector.y = y; vector.z = z; return vector; }
        public static Vector3 SetX(this Vector3 vector, float x) { vector.x = x; return vector; }
        public static Vector3 SetY(this Vector3 vector, float y) { vector.y = y; return vector; }
        public static Vector3 SetZ(this Vector3 vector, float z) { vector.z = z; return vector; }
        public static Vector3 SetRef(ref this Vector3 vector, float x, float y, float z) { vector.x = x; vector.y = y; vector.z = z; return vector; }
        public static Vector3 SetXYRef(ref this Vector3 vector, float x, float y) { vector.x = x; vector.y = y; return vector; }
        public static Vector3 SetXZRef(ref this Vector3 vector, float x, float z) { vector.x = x; vector.z = z; return vector; }
        public static Vector3 SetYZRef(ref this Vector3 vector, float y, float z) { vector.y = y; vector.z = z; return vector; }
        public static Vector3 SetXRef(ref this Vector3 vector, float x) { vector.x = x; return vector; }
        public static Vector3 SetYRef(ref this Vector3 vector, float y) { vector.y = y; return vector; }
        public static Vector3 SetZRef(ref this Vector3 vector, float z) { vector.z = z; return vector; }

        public static Vector2 SignedAdd(this Vector2 vector, float i)
        {
            float revX = Mathf.Sign(vector.x);
            float revY = Mathf.Sign(vector.y);

            return new Vector2(vector.x + revX * i, vector.y + revY * i);
        }

        public static Vector2 Invert(this Vector2 vector) => new Vector3(1 / vector.x, 1 / vector.y);
        public static Vector3 Invert(this Vector3 vector) => new Vector3(1 / vector.x, 1 / vector.y, 1 / vector.z);

        public static Vector2 ReplaceNaN(this Vector2 vector, float value)
        {
            vector = new Vector2(float.IsNaN(vector.x) ? value : vector.x, float.IsNaN(vector.y) ? value : vector.y);
            return vector;
        }

        public static Color Vector4AsColor(this Vector4 vector)
        {
            return new Color(vector.x, vector.y, vector.z, vector.w);
        }

        public static bool FacingUp(this Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, Vector2.up);
            return signedAngle <= 67.5f && signedAngle > -67.5f;
        }
        public static bool FacingDown(this Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, Vector2.down);
            return signedAngle <= 67.5f && signedAngle > -67.5f;
        }
        public static bool FacingRight(this Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, Vector2.right);
            return signedAngle <= 22.5f && signedAngle > -22.5f;
        }
        public static bool FacingLeft(this Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, Vector2.left);
            return signedAngle <= 22.5f && signedAngle > -22.5f;
        }

        public static Vector2 GetFacingDirection(this Vector2 direction)
        {
            if (direction.FacingDown())
                return Vector2.down;
            else if (direction.FacingUp())
                return Vector2.up;
            else if (direction.FacingLeft())
                return Vector2.left;
            else if (direction.FacingRight())
                return Vector2.right;

            return Vector2.zero;
        }

        public static Vector2 RoundToDirection(this Vector2 direction)
        {
            if (direction.x.Abs() > direction.y.Abs())
            {
                return direction.SetY(0).normalized;
            }
            else// if (direction.y.Abs() > direction.x.Abs())
            {
                return direction.SetX(0).normalized;
            }

            //return Vector2.zero;
        }
    }

    public static class ColorExtensions
    {
        public static Color SetHue(this Color color, float Hue)
        {
            Hue = Mathf.Repeat(Hue, 1);
            return Color.HSVToRGB(Hue, color.Saturation(), color.Brightness()).SetAlpha(color.a);
        }
        public static Color SetSaturation(this Color color, float saturation)
        {
            saturation = Mathf.Clamp01(saturation);
            return Color.HSVToRGB(color.Hue(), saturation, color.Brightness()).SetAlpha(color.a);
        }
        public static Color SetBrightness(this Color color, float brightness)
        {
            brightness = Mathf.Clamp01(brightness);
            return Color.HSVToRGB(color.Hue(), color.Saturation(), brightness).SetAlpha(color.a);
        }
        public static float Hue(this Color color)
        {
            Color.RGBToHSV(color, out float H, out _, out _);
            return H;
        }
        public static float Saturation(this Color color)
        {
            Color.RGBToHSV(color, out _, out float H, out _);
            return H;
        }
        public static float Brightness(this Color color)
        {
            Color.RGBToHSV(color, out _, out _, out float H);
            return H;
        }

        public static Color GetHueColor(this Color color)
        {
            /*float Saturation = color.Saturation();
            if (Saturation == 0)
                return Color.red;*/

            float Hue = color.Hue();
            return Color.red.SetHue(Hue);
        }

        public static Color SetAlpha(this Color color, float Alpha)
        {
            color.a = Alpha;
            return color;
        }
        public static Color SetRed(this Color color, float R)
        {
            color.r = R;
            return color;
        }
        public static Color SetBlue(this Color color, float G)
        {
            color.b = G;
            return color;
        }
        public static Color SetGreen(this Color color, float B)
        {
            color.g = B;
            return color;
        }
        public static bool RGBMatches(this Color A, Color B) => A.r == B.r && A.g == B.g && A.b == B.b;

        public static Color WithAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);
        public static Color MultiplyRGB(this Color c, float m) => new Color(c.r * m, c.g * m, c.b * m, c.a);
        public static Color MultiplyRGB(this Color c, Color m) => new Color(c.r * m.r, c.g * m.g, c.b * m.b, c.a);
        public static Color MultiplyA(this Color c, float m) => new Color(c.r, c.g, c.b, c.a * m);

        public static Vector4 ColorAsVector4(this Color color)
        {
            return new Vector4(color.r, color.g, color.b, color.a);
        }

        public static Color Sepia(this Color color)
        {
            color.r = (color.r * .393f) + (color.g * .769f) + (color.b * .189f);
            color.g = (color.r * .349f) + (color.g * .686f) + (color.b * .168f);
            color.b = (color.r * .272f) + (color.g * .534f) + (color.b * .131f);
            return color;
        }

        public static Color Invert(this Color color)
        {
            color = new Color(1 - color.r, 1 - color.g, 1 - color.b, color.a);
            return color;
        }

        public static bool EqualsRGB(this Color A, Color B)
        {
            return A.r == B.r && A.g == B.g && A.b == B.b;
        }
    }

    public static class QuaternionExtensions
    {
		
    }

    public static class LayerMaskExtensions
    {
        public static bool Contains(this LayerMask mask, int layer) => (mask & (1 << layer)) != 0;
    }

    public static class CameraExtensions
    {
        public static bool WithinCamUp(this Camera camera, Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, (Vector2)camera.transform.up);
            return signedAngle <= 67.5f && signedAngle > -67.5f;
        }
        public static bool WithinCamDown(this Camera camera, Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, -(Vector2)camera.transform.up);
            return signedAngle <= 67.5f && signedAngle > -67.5f;
        }
        public static bool WithinCamRight(this Camera camera, Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, (Vector2)camera.transform.right);
            return signedAngle <= 22.5f && signedAngle > -22.5f;
        }
        public static bool WithinCamLeft(this Camera camera, Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, -(Vector2)camera.transform.right);
            return signedAngle <= 22.5f && signedAngle > -22.5f;
        }

        public static bool SameCamDirection(this Camera camera, Vector2 direction1, Vector2 direction2) =>
            (camera.WithinCamUp(direction1) && camera.WithinCamUp(direction2)) ||
            (camera.WithinCamDown(direction1) && camera.WithinCamDown(direction2)) ||
            (camera.WithinCamRight(direction1) && camera.WithinCamRight(direction2)) ||
            (camera.WithinCamLeft(direction1) && camera.WithinCamLeft(direction2));

        public static bool OppositeCamDirection(this Camera camera, Vector2 direction1, Vector2 direction2) =>
            (camera.WithinCamUp(direction1) && camera.WithinCamDown(direction2)) ||
            (camera.WithinCamDown(direction1) && camera.WithinCamUp(direction2)) ||
            (camera.WithinCamRight(direction1) && camera.WithinCamLeft(direction2)) ||
            (camera.WithinCamLeft(direction1) && camera.WithinCamRight(direction2));

        public static string GetCameraDirection(this Camera camera, Vector2 direction)
        {
            if (camera.WithinCamDown(direction))
                return "Down";// Vector2.down;
            else if (camera.WithinCamUp(direction))
                return "Up";// Vector2.up;
            else if (camera.WithinCamRight(direction))
                return "Right";// Vector2.right;
            else if (camera.WithinCamLeft(direction))
                return "Left";// Vector2.left;

            return "None";// Vector2.zero;
        }
    }

    public static class TransformExtensions
    {
        public static bool OrientedUp(this Transform transform, Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, (Vector2)transform.up);
            return signedAngle <= 67.5f && signedAngle > -67.5f;
        }
        public static bool OrientedDown(this Transform transform, Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, -(Vector2)transform.up);
            return signedAngle <= 67.5f && signedAngle > -67.5f;
        }
        public static bool OrientedRight(this Transform transform, Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, (Vector2)transform.right);
            return signedAngle <= 22.5f && signedAngle > -22.5f;
        }
        public static bool OrientedLeft(this Transform transform, Vector2 direction)
        {
            float signedAngle = Vector2.SignedAngle(direction, -(Vector2)transform.right);
            return signedAngle <= 22.5f && signedAngle > -22.5f;
        }

        public static string GetOrientation(this Transform transform, Vector2 direction)
        {
            if (transform.OrientedDown(direction))
                return "Down";// Vector2.down;
            else if (transform.OrientedUp(direction))
                return "Up";// Vector2.up;
            else if (transform.OrientedRight(direction))
                return "Right";// Vector2.right;
            else if (transform.OrientedLeft(direction))
                return "Left";// Vector2.left;

            return "None";// Vector2.zero;
        }
    }
}