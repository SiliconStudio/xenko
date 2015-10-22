// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    internal struct VertexAttrib : IEquatable<VertexAttrib>
    {
        public int VertexBufferId;
        public int Index;
        public int Size;
        public bool IsInteger;
        public VertexAttribPointerType Type;
        public bool Normalized;
        public int Stride;
        public IntPtr Offset;
        public string AttributeName;

        public bool Equals(VertexAttrib other)
        {
            return VertexBufferId == other.VertexBufferId && Index == other.Index && Size == other.Size && IsInteger.Equals(other.IsInteger) && Type == other.Type && Normalized.Equals(other.Normalized) && Stride == other.Stride && Offset.Equals(other.Offset) && string.Equals(AttributeName, other.AttributeName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VertexAttrib && Equals((VertexAttrib) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = VertexBufferId;
                hashCode = (hashCode*397) ^ Index;
                hashCode = (hashCode*397) ^ Size;
                hashCode = (hashCode*397) ^ IsInteger.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Type;
                hashCode = (hashCode*397) ^ Normalized.GetHashCode();
                hashCode = (hashCode*397) ^ Stride;
                hashCode = (hashCode*397) ^ Offset.GetHashCode();
                hashCode = (hashCode*397) ^ AttributeName.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(VertexAttrib left, VertexAttrib right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexAttrib left, VertexAttrib right)
        {
            return !left.Equals(right);
        }
    }
}

#endif