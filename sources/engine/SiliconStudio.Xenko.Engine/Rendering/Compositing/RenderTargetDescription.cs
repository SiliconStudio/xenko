// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public struct RenderTargetDescription : IEquatable<RenderTargetDescription>
    {
        public IRenderTargetSemantic Semantic;

        public PixelFormat Format;

        public bool Equals(RenderTargetDescription other)
        {
            return Semantic.GetType() == other.Semantic.GetType() && Format == other.Format;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is RenderTargetDescription && Equals((RenderTargetDescription)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Semantic?.GetType().GetHashCode() ?? 0) * 397) ^ (int)Format;
            }
        }

        public static bool operator ==(RenderTargetDescription left, RenderTargetDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RenderTargetDescription left, RenderTargetDescription right)
        {
            return !left.Equals(right);
        }
    }
}
