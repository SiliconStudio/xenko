// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.InteropServices;

using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Describes a blend state.
    /// </summary>
    [DataContract]
    [StructLayout(LayoutKind.Sequential)]
    public struct BlendStateDescription : IEquatable<BlendStateDescription>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlendStateDescription"/> class.
        /// </summary>
        /// <param name="sourceBlend">The source blend.</param>
        /// <param name="destinationBlend">The destination blend.</param>
        public BlendStateDescription(Blend sourceBlend, Blend destinationBlend) : this()
        {
            SetDefaults();
            RenderTargets[0].BlendEnable = true;
            RenderTargets[0].ColorSourceBlend = sourceBlend;
            RenderTargets[0].ColorDestinationBlend = destinationBlend;
            RenderTargets[0].AlphaSourceBlend = sourceBlend;
            RenderTargets[0].AlphaDestinationBlend = destinationBlend;
        }

        /// <summary>
        /// Setup this blend description with defaults value.
        /// </summary>
        public void SetDefaults()
        {
            RenderTargets = new BlendStateRenderTargetDescription[8];

            AlphaToCoverageEnable = false;
            IndependentBlendEnable = false;

            for (int i = 0; i < RenderTargets.Length; i++)
            {
                RenderTargets[i].BlendEnable = false;
                RenderTargets[i].ColorSourceBlend = Blend.One;
                RenderTargets[i].ColorDestinationBlend = Blend.Zero;
                RenderTargets[i].ColorBlendFunction = BlendFunction.Add;

                RenderTargets[i].AlphaSourceBlend = Blend.One;
                RenderTargets[i].AlphaDestinationBlend = Blend.Zero;
                RenderTargets[i].AlphaBlendFunction = BlendFunction.Add;

                RenderTargets[i].ColorWriteChannels = ColorWriteChannels.All;
            }
        }

        /// <summary>
        /// Gets default values for this instance.
        /// </summary>
        public static BlendStateDescription Default
        {
            get
            {
                var desc = new BlendStateDescription();
                desc.SetDefaults();
                return desc;
            }
        }

        /// <summary>
        /// Determines whether or not to use alpha-to-coverage as a multisampling technique when setting a pixel to a rendertarget. 
        /// </summary>
        public bool AlphaToCoverageEnable;

        /// <summary>
        /// Set to true to enable independent blending in simultaneous render targets.  If set to false, only the RenderTarget[0] members are used. RenderTarget[1..7] are ignored. 
        /// </summary>
        public bool IndependentBlendEnable;

        /// <summary>
        /// An array of render-target-blend descriptions (see <see cref="BlendStateRenderTargetDescription"/>); these correspond to the eight rendertargets  that can be set to the output-merger stage at one time. 
        /// </summary>
        public BlendStateRenderTargetDescription[] RenderTargets;

        /// <inheritdoc/>
        public bool Equals(BlendStateDescription other)
        {
            return AlphaToCoverageEnable.Equals(other.AlphaToCoverageEnable)
                && IndependentBlendEnable.Equals(other.IndependentBlendEnable)
                && ArrayExtensions.ArraysEqual(RenderTargets, other.RenderTargets);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BlendStateDescription && Equals((BlendStateDescription)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = AlphaToCoverageEnable.GetHashCode();
                hashCode = (hashCode*397) ^ IndependentBlendEnable.GetHashCode();
                hashCode = (hashCode*397) ^ RenderTargets.ComputeHash();
                return hashCode;
            }
        }
    }
}