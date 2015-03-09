// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// A context containing information for UI layouting.
    /// </summary>
    public class LayoutingContext : IEquatable<LayoutingContext>
    {
        /// <summary>
        /// The resolution of the output target.
        /// </summary>
        public Vector2 RealResolution { get; private set; }

        /// <summary>
        /// The virtual resolution of the UI.
        /// </summary>
        public Vector3 VirtualResolution { get; internal set; }

        /// <summary>
        /// The ratio between the real and virtual resolution (=real/virtual)
        /// </summary>
        public Vector2 RealVirtualResolutionRatio { get; private set; }

        /// <summary>
        /// Determine if two <see cref="LayoutingContext"/> are equals.
        /// </summary>
        /// <param name="other">the other context</param>
        /// <returns><value>True</value> if the two contexts are equals</returns>
        public bool Equals(LayoutingContext other)
        {
            return RealResolution.Equals(other.RealResolution) && VirtualResolution.Equals(other.VirtualResolution);
        }

        internal void CalculateRealResolutionAndRatio(float realHeightEstimate)
        {
            // Calculate the real resolution so that the RealVirtualResolution is uniform (same value for X and Y)
            // -> This ensure that freetype does not produce deformed characters.
            var virtualResolutionRatio = VirtualResolution.X / VirtualResolution.Y;
            RealResolution = new Vector2(virtualResolutionRatio * realHeightEstimate, realHeightEstimate);
            RealVirtualResolutionRatio = new Vector2(RealResolution.Y / VirtualResolution.Y);
        }
    }
}