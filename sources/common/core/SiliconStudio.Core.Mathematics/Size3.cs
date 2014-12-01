// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under MIT License. See LICENSE.md for details.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Runtime.InteropServices;

namespace SiliconStudio.Core.Mathematics
{
    /// <summary>
    /// Structure providing Width, Height and Depth.
    /// </summary>
    [DataContract("!Size3")]
    [DataStyle(DataStyle.Compact)]
    [StructLayout(LayoutKind.Sequential)]
    public struct Size3 : IEquatable<Size3>
    {
        /// <summary>
        /// A zero size with (width, height, depth) = (0,0,0)
        /// </summary>
        public static readonly Size3 Zero = new Size3(0, 0, 0);

        /// <summary>
        /// A zero size with (width, height, depth) = (0,0,0)
        /// </summary>
        public static readonly Size3 Empty = Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="Size3" /> struct.
        /// </summary>
        /// <param name="width">The x.</param>
        /// <param name="height">The y.</param>
        /// <param name="depth">The depth.</param>
        public Size3(int width, int height, int depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
        }

        /// <summary>
        /// Width.
        /// </summary>
        [DataMember(0)]
        public int Width;

        /// <summary>
        /// Height.
        /// </summary>
        [DataMember(1)]
        public int Height;

        /// <summary>
        /// Height.
        /// </summary>
        [DataMember(2)]
        public int Depth;

        public bool Equals(Size3 other)
        {
            return Width == other.Width && Height == other.Height && Depth == other.Depth;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Size3 && Equals((Size3)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Width;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ Depth;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", Width, Height, Depth);
        }
    }
}