// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Describes a custom vertex format structure that contains position, color and 10 texture coordinates information. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalMultiTexture : IEquatable<VertexPositionNormalMultiTexture>, IVertexWindable
    {
        /// <summary>
        /// Initializes a new <see cref="SiliconStudio.Paradox.Graphics.VertexPositionNormalTexture"/> instance.
        /// </summary>
        /// <param name="position">The position of this vertex.</param>
        /// <param name="normal">The vertex normal.</param>
        /// <param name="textureCoordinate">UV texture coordinates.</param>
        public VertexPositionNormalMultiTexture(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
            : this()
        {
            Position = position;
            Normal = normal;
            TextureCoordinate0 = textureCoordinate;
            TextureCoordinate1 = textureCoordinate;
            TextureCoordinate2 = textureCoordinate;
            TextureCoordinate3 = textureCoordinate;
            TextureCoordinate4 = textureCoordinate;
            TextureCoordinate5 = textureCoordinate;
            TextureCoordinate6 = textureCoordinate;
            TextureCoordinate7 = textureCoordinate;
            TextureCoordinate8 = textureCoordinate;
            TextureCoordinate9 = textureCoordinate;
        }

        /// <summary>
        /// XYZ position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// UV texture coordinates.
        /// </summary>
        public Vector2 TextureCoordinate0;
        public Vector2 TextureCoordinate1;
        public Vector2 TextureCoordinate2;
        public Vector2 TextureCoordinate3;
        public Vector2 TextureCoordinate4;
        public Vector2 TextureCoordinate5;
        public Vector2 TextureCoordinate6;
        public Vector2 TextureCoordinate7;
        public Vector2 TextureCoordinate8;
        public Vector2 TextureCoordinate9;

        /// <summary>
        /// Defines structure byte size.
        /// </summary>
        public static readonly int Size = 96;


        /// <summary>
        /// The vertex layout of this struct.
        /// </summary>
        public static readonly VertexDeclaration Layout = new VertexDeclaration(
            VertexElement.Position<Vector3>(),
            VertexElement.Normal<Vector3>(),
            VertexElement.TextureCoordinate<Vector2>(0),
            VertexElement.TextureCoordinate<Vector2>(1),
            VertexElement.TextureCoordinate<Vector2>(2),
            VertexElement.TextureCoordinate<Vector2>(3),
            VertexElement.TextureCoordinate<Vector2>(4),
            VertexElement.TextureCoordinate<Vector2>(5),
            VertexElement.TextureCoordinate<Vector2>(6),
            VertexElement.TextureCoordinate<Vector2>(7),
            VertexElement.TextureCoordinate<Vector2>(8),
            VertexElement.TextureCoordinate<Vector2>(9)
            );


        public bool Equals(VertexPositionNormalMultiTexture other)
        {
            return Position.Equals(other.Position) && Normal.Equals(other.Normal)
                   && TextureCoordinate0.Equals(other.TextureCoordinate0)
                   && TextureCoordinate1.Equals(other.TextureCoordinate1)
                   && TextureCoordinate2.Equals(other.TextureCoordinate2)
                   && TextureCoordinate3.Equals(other.TextureCoordinate3)
                   && TextureCoordinate4.Equals(other.TextureCoordinate4)
                   && TextureCoordinate5.Equals(other.TextureCoordinate5)
                   && TextureCoordinate6.Equals(other.TextureCoordinate6)
                   && TextureCoordinate7.Equals(other.TextureCoordinate7)
                   && TextureCoordinate8.Equals(other.TextureCoordinate8)
                   && TextureCoordinate9.Equals(other.TextureCoordinate9);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VertexPositionNormalMultiTexture && Equals((VertexPositionNormalMultiTexture)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate0.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate1.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate2.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate3.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate4.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate5.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate6.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate7.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate8.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate9.GetHashCode();
                return hashCode;
            }
        }

        public void FlipWinding()
        {
            TextureCoordinate0.X = (1.0f - TextureCoordinate0.X);
            TextureCoordinate1.X = (1.0f - TextureCoordinate1.X);
            TextureCoordinate2.X = (1.0f - TextureCoordinate2.X);
            TextureCoordinate3.X = (1.0f - TextureCoordinate3.X);
            TextureCoordinate4.X = (1.0f - TextureCoordinate4.X);
            TextureCoordinate5.X = (1.0f - TextureCoordinate5.X);
            TextureCoordinate6.X = (1.0f - TextureCoordinate6.X);
            TextureCoordinate7.X = (1.0f - TextureCoordinate7.X);
            TextureCoordinate8.X = (1.0f - TextureCoordinate8.X);
            TextureCoordinate9.X = (1.0f - TextureCoordinate9.X);
        }

        public static bool operator ==(VertexPositionNormalMultiTexture left, VertexPositionNormalMultiTexture right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexPositionNormalMultiTexture left, VertexPositionNormalMultiTexture right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, Normal: {1}, Texcoord: {2}", Position, Normal, TextureCoordinate0);
        }
    }
}