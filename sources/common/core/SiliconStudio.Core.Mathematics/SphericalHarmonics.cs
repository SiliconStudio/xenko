// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core.Mathematics
{
    /// <summary>
    /// A representation of a sphere of values via Spherical Harmonics (SH).
    /// </summary>
    /// <typeparam name="TDataType">The type of data contained by the sphere</typeparam>
    [DataContract("SphericalHarmonicsGeneric")]
    public abstract class SphericalHarmonics<TDataType>
    {
        /// <summary>
        /// The maximum order supported.
        /// </summary>
        public const int MaximumOrder = 5;

        /// <summary>
        /// A function of the spherical base.
        /// </summary>
        /// <param name="direction">The direction</param>
        /// <returns>The value in the direction</returns>
        public delegate float SphericalBase(Vector3 direction);

        /// <summary>
        /// The spherical bases of order 1, 2, ..., <see cref="MaximumOrder"/>.
        /// </summary>
        public static readonly SphericalBase[] Bases =
        {
            Y00, 
            Y1M1, Y10, Y1P1,
            Y2M2, Y2M1, Y20, Y2P1, Y2P2,
            Y3M3, Y3M2, Y3M1, Y30, Y3P1, Y3P2, Y3P3,
            Y4M4, Y4M3, Y4M2, Y4M1, Y40, Y4P1, Y4P2, Y4P3, Y4P4
        };

        private int order;

        /// <summary>
        /// The order of calculation of the spherical harmonic.
        /// </summary>
        [DataMember(0)]
        public int Order
        {
            get { return order; }
            internal set
            {
                if(order>5)
                    throw new NotSupportedException("Only orders inferior or equal to 5 are supported");
                
                order = Math.Max(1, value);
            }
        }

        /// <summary>
        /// Get the coefficients defining the spherical harmonics (the spherical coordinates x{l,m} multiplying the spherical base Y{l,m}).
        /// </summary>
        [DataMember(1)]
        public TDataType[] Coefficients { get; internal set; }

        /// <summary>
        /// Creates a null spherical harmonics (for serialization).
        /// </summary>
        internal SphericalHarmonics()
        {
        }

        /// <summary>
        /// The desired order to
        /// </summary>
        /// <param name="order"></param>
        protected SphericalHarmonics(int order)
        {
            this.order = order;
           Coefficients = new TDataType[order * order]; 
        }

        /// <summary>
        /// Evaluate the value of the spherical harmonics in the provided direction.
        /// </summary>
        /// <param name="direction">The direction</param>
        /// <returns>The value of the spherical harmonics in the direction</returns>
        public abstract TDataType Evaluate(Vector3 direction);

        /// <summary>
        /// Returns the coefficient x{l,m} of the spherical harmonics (the {l,m} spherical coordinate corresponding to the spherical base Y{l,m}).
        /// </summary>
        /// <param name="l">the l index of the coefficient</param>
        /// <param name="m">the m index of the coefficient</param>
        /// <returns>the value of the coefficient</returns>
        public TDataType this[int l, int m]
        {
            get
            {
                CheckIndicesValidity(l, m, order);
                return Coefficients[LmToCoefficientIndex(l, m)];
            }
            set
            {
                CheckIndicesValidity(l, m, order); 
                Coefficients[LmToCoefficientIndex(l, m)] = value;
            }
        }

        /// <summary>
        /// Gets the {l, m}-indexed base function of the Spherical Harmonics.
        /// </summary>
        /// <param name="l">the l index</param>
        /// <param name="m">the m index</param>
        /// <returns>The corresponding base function</returns>
        public static SphericalBase GetBase(int l, int m)
        {
            CheckIndicesValidity(l, m, MaximumOrder);
            return Bases[LmToCoefficientIndex(l, m)];
        }

        // ReSharper disable UnusedParameter.Local
        private static void CheckIndicesValidity(int l, int m, int maxOrder)
        // ReSharper restore UnusedParameter.Local
        {
            if (l > maxOrder - 1)
                throw new IndexOutOfRangeException("'l' parameter should be between '0' and '{0}' (order-1).".ToFormat(maxOrder-1));

            if(Math.Abs(m) > l)
                throw new IndexOutOfRangeException("'m' parameter should be between '-l' and '+l'.");
        }

        private static int LmToCoefficientIndex(int l, int m)
        {
            return l * l + l + m;
        }
        
        private static float Y00(Vector3 dir)
        {
            throw new NotImplementedException();
        }

        private static float Y1M1(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y10(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y1P1(Vector3 dir)
        {
            throw new NotImplementedException();
        }

        private static float Y2M2(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y2M1(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y20(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y2P1(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y2P2(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        
        private static float Y3M3(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y3M2(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y3M1(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y30(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y3P1(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y3P2(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y3P3(Vector3 dir)
        {
            throw new NotImplementedException();
        }

        private static float Y4M4(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y4M3(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y4M2(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y4M1(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y40(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y4P1(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y4P2(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y4P3(Vector3 dir)
        {
            throw new NotImplementedException();
        }
        private static float Y4P4(Vector3 dir)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A spherical harmonics representation of a cubemap.
    /// </summary>
    [DataContract("SphericalHarmonics")]
    public class SphericalHarmonics : SphericalHarmonics<Color3>
    {
        internal SphericalHarmonics()
        {
        }

        /// <summary>
        /// Create a new instance of Spherical Harmonics of provided order.
        /// </summary>
        /// <param name="order">The order of the harmonics</param>
        public SphericalHarmonics(int order)
            : base(order)
        {
        }

        public override Color3 Evaluate(Vector3 direction)
        {
            var data = new Color3();
            for (int i = 0; i < Coefficients.Length; i++)
                data += Coefficients[i] * Bases[i](direction);

            return data;
        }
    }
}