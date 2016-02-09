using System.Runtime.CompilerServices;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Extensions
{
    using WindowsPoint = System.Windows.Point;
    using WindowsVector = System.Windows.Vector;

    public static class MathExtensions
    {
        /// <summary>
        /// Converts a <see cref="WindowsPoint"/> to a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this WindowsPoint point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        /// <summary>
        /// Converts a <see cref="WindowsVector"/> to a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this WindowsVector vector)
        {
            return new Vector2((float)vector.X, (float)vector.Y);
        }

        /// <summary>
        /// Converts a <see cref="Vector2"/> to a <see cref="WindowsPoint"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WindowsPoint ToWindowsPoint(this Vector2 point)
        {
            return new WindowsPoint(point.X, point.Y);
        }

        /// <summary>
        /// Converts a <see cref="Vector2"/> to a <see cref="WindowsVector"/>.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WindowsVector ToWindowsVector(this Vector2 vector)
        {
            return new WindowsVector(vector.X, vector.Y);
        }
    }
}
