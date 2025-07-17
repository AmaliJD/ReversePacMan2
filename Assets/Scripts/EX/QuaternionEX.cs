using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EX
{
    public static class QuaternionEX
    {
        public static Quaternion Rotate(this Quaternion q, float x, float y, float z)
        {
            return q * Quaternion.Euler(x, y, z);
        }
    }
}
