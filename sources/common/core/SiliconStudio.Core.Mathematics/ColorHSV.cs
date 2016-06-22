// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under MIT License. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SiliconStudio.Core.Mathematics
{
    /// <summary>
    /// Represents a color in the form of Hue, Saturation, Value, Alpha.
    /// </summary>
    [DataContract("ColorHSV")]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ColorHSV : IEquatable<ColorHSV>, IFormattable
    {
        private const string ToStringFormat = "Hue:{0} Saturation:{1} Value:{2} Alpha:{3}";

        /// <summary>
        /// The Hue of the color.
        /// </summary>
        [DataMember(0)]
        public float H;

        /// <summary>
        /// The Saturation of the color.
        /// </summary>
        [DataMember(1)]
        public float S;

        /// <summary>
        /// The Value of the color.
        /// </summary>
        [DataMember(2)]
        public float V;

        /// <summary>
        /// The alpha component of the color.
        /// </summary>
        [DataMember(3)]
        public float A;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorHSV"/> struct.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="s">The s.</param>
        /// <param name="v">The v.</param>
        /// <param name="a">A.</param>
        public ColorHSV(float h, float s, float v, float a)
        {
            H = h;
            S = s;
            V = v;
            A = a;
        }

        /// <summary>
        /// Converts the color into a three component vector.
        /// </summary>
        /// <returns>A three component vector containing the red, green, and blue components of the color.</returns>
        public Color4 ToColor()
        {
            var hdiv = (int)(H / 60);
            int hi = hdiv % 6;
            float f = H / 60 - hdiv;

            float v = V;
            float p = V * (1 - S);
            float q = V * (1 - f * S);
            float t = V * (1 - (1 - f) * S);

            switch (hi)
            {
                case 0:
                    return new Color4(v, t, p, A);
                case 1:
                    return new Color4(q, v, p, A);
                case 2:
                    return new Color4(p, v, t, A);
                case 3:
                    return new Color4(p, q, v, A);
                case 4:
                    return new Color4(t, p, v, A);
                default:
                    return new Color4(v, p, q, A);
            }
        }

        /// <summary>
        /// Converts the color into a HSV color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>A HSV color</returns>
        public static ColorHSV FromColor(Color4 color)
        {
            float max = Math.Max(color.R, Math.Max(color.G, color.B));
            float min = Math.Min(color.R, Math.Min(color.G, color.B));

            float delta = max - min;
            float h = 0.0f;

            if (delta > 0.0f)
            {
                if (max == color.R && max != color.G)
                    h += (color.G - color.B) / delta;
                if (max == color.G && max != color.B)
                    h += (2.0f + (color.B - color.R) / delta);
                if (max == color.B && max != color.R)
                    h += (4.0f + (color.R - color.G) / delta);
                h *= 60.0f;
            }

            return new ColorHSV(h, (max != 0.0f) ? 1.0f - min / max : 0.0f, max, color.A);
        }

        /// <inheritdoc/>
        public bool Equals(ColorHSV other)
        {
            return other.H.Equals(H) && other.S.Equals(S) && other.V.Equals(V) && other.A.Equals(A);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(ColorHSV)) return false;
            return Equals((ColorHSV)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = H.GetHashCode();
                result = (result * 397) ^ S.GetHashCode();
                result = (result * 397) ^ V.GetHashCode();
                result = (result * 397) ^ A.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, ToStringFormat, H, S, V, A);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, ToStringFormat,
                                 H.ToString(format, formatProvider),
                                 S.ToString(format, formatProvider),
                                 V.ToString(format, formatProvider),
                                 A.ToString(format, formatProvider));
        }
    }
}
