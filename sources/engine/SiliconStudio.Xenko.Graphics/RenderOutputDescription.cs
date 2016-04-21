// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Describes render targets and depth stencil output formats.
    /// </summary>
    public struct RenderOutputDescription : IEquatable<RenderOutputDescription>
    {
        // Render targets
        public int RenderTargetCount;
        public PixelFormat RenderTargetFormat0;
        public PixelFormat RenderTargetFormat1;
        public PixelFormat RenderTargetFormat2;
        public PixelFormat RenderTargetFormat3;
        public PixelFormat RenderTargetFormat4;
        public PixelFormat RenderTargetFormat5;
        public PixelFormat RenderTargetFormat6;
        public PixelFormat RenderTargetFormat7;

        public PixelFormat DepthStencilFormat;

        public MSAALevel MultiSampleLevel;

        public RenderOutputDescription(PixelFormat renderTargetFormat, PixelFormat depthStencilFormat = PixelFormat.None, MSAALevel multiSampleLevel = MSAALevel.None) : this()
        {
            RenderTargetCount = renderTargetFormat != PixelFormat.None ? 1 : 0;
            RenderTargetFormat0 = renderTargetFormat;
            DepthStencilFormat = depthStencilFormat;
            MultiSampleLevel = multiSampleLevel;
        }

        public unsafe void CaptureState(CommandList commandList)
        {
            DepthStencilFormat = commandList.DepthStencilBuffer != null ? commandList.DepthStencilBuffer.ViewFormat : PixelFormat.None;
            MultiSampleLevel = commandList.DepthStencilBuffer != null ? commandList.DepthStencilBuffer.MultiSampleLevel : MSAALevel.None;

            RenderTargetCount = commandList.RenderTargetCount;
            fixed (PixelFormat* renderTargetFormat0 = &RenderTargetFormat0)
            {
                var renderTargetFormat = renderTargetFormat0;
                for (int i = 0; i < RenderTargetCount; ++i)
                {
                    *renderTargetFormat++ = commandList.RenderTargets[i].ViewFormat;
                    MultiSampleLevel = commandList.RenderTargets[i].MultiSampleLevel; // multisample should all be equal
                }
            }
        }

        public bool Equals(RenderOutputDescription other)
        {
            return RenderTargetCount == other.RenderTargetCount
                   && RenderTargetFormat0 == other.RenderTargetFormat0
                   && RenderTargetFormat1 == other.RenderTargetFormat1
                   && RenderTargetFormat2 == other.RenderTargetFormat2
                   && RenderTargetFormat3 == other.RenderTargetFormat3
                   && RenderTargetFormat4 == other.RenderTargetFormat4
                   && RenderTargetFormat5 == other.RenderTargetFormat5
                   && RenderTargetFormat6 == other.RenderTargetFormat6
                   && RenderTargetFormat7 == other.RenderTargetFormat7
                   && DepthStencilFormat == other.DepthStencilFormat;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is RenderOutputDescription && Equals((RenderOutputDescription)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = RenderTargetCount;
                hashCode = (hashCode*397) ^ (int)RenderTargetFormat0;
                hashCode = (hashCode*397) ^ (int)RenderTargetFormat1;
                hashCode = (hashCode*397) ^ (int)RenderTargetFormat2;
                hashCode = (hashCode*397) ^ (int)RenderTargetFormat3;
                hashCode = (hashCode*397) ^ (int)RenderTargetFormat4;
                hashCode = (hashCode*397) ^ (int)RenderTargetFormat5;
                hashCode = (hashCode*397) ^ (int)RenderTargetFormat6;
                hashCode = (hashCode*397) ^ (int)RenderTargetFormat7;
                hashCode = (hashCode*397) ^ (int)DepthStencilFormat;
                return hashCode;
            }
        }

        public static bool operator ==(RenderOutputDescription left, RenderOutputDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RenderOutputDescription left, RenderOutputDescription right)
        {
            return !left.Equals(right);
        }
    }
}