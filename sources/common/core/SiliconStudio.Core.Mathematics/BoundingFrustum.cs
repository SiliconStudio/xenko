// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under MIT License. See LICENSE.md for details.
//
﻿// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
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
namespace SiliconStudio.Core.Mathematics
{
    public struct BoundingFrustum
    {
        public Plane Plane1;
        public Plane Plane2;
        public Plane Plane3;
        public Plane Plane4;
        public Plane Plane5;
        public Plane Plane6;

        public BoundingFrustum(ref Matrix matrix)
        {
            // Left
            Plane1 = Plane.Normalize(new Plane(
                matrix.M14 + matrix.M11,
                matrix.M24 + matrix.M21,
                matrix.M34 + matrix.M31,
                matrix.M44 + matrix.M41));

            // Right
            Plane2 = Plane.Normalize(new Plane(
                matrix.M14 - matrix.M11,
                matrix.M24 - matrix.M21,
                matrix.M34 - matrix.M31,
                matrix.M44 - matrix.M41));

            // Top
            Plane3 = Plane.Normalize(new Plane(
                matrix.M14 - matrix.M12,
                matrix.M24 - matrix.M22,
                matrix.M34 - matrix.M32,
                matrix.M44 - matrix.M42));

            // Bottom
            Plane4 = Plane.Normalize(new Plane(
                matrix.M14 + matrix.M12,
                matrix.M24 + matrix.M22,
                matrix.M34 + matrix.M32,
                matrix.M44 + matrix.M42));

            // Near
            Plane5 = Plane.Normalize(new Plane(
                matrix.M13,
                matrix.M23,
                matrix.M33,
                matrix.M43));

            // Far
            Plane6 = Plane.Normalize(new Plane(
                matrix.M14 - matrix.M13,
                matrix.M24 - matrix.M23,
                matrix.M34 - matrix.M33,
                matrix.M44 - matrix.M43));
        }
    }
}