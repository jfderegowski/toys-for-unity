using System.Runtime.CompilerServices;
using UnityEngine;

namespace fefek5.Toys.Runtime.Extensions
{
    public static class QuaternionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ToUnityQuaternion(this System.Numerics.Quaternion quaternion) => 
            new(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Numerics.Quaternion ToSystemQuaternion(this Quaternion quaternion) => 
            new(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        
        public static Quaternion Round(this Quaternion quaternion, int decimalPlaces)
        {
            var multiplier = Mathf.Pow(10, decimalPlaces);
        
            quaternion.x = Mathf.Round(quaternion.x * multiplier) / multiplier;
            quaternion.y = Mathf.Round(quaternion.y * multiplier) / multiplier;
            quaternion.z = Mathf.Round(quaternion.z * multiplier) / multiplier;
            quaternion.w = Mathf.Round(quaternion.w * multiplier) / multiplier;

            return quaternion;
        }
    }
}