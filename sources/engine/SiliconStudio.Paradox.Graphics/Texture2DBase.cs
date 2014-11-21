// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
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
using System.IO;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A Texture 2D frontend to <see cref="SharpDX.Direct3D11.Texture2D"/>.
    /// </summary>
    public abstract partial class Texture2DBase : Texture
    {
        protected Texture2DBase()
        {
        }

        public static TextureDescription NewDescription(int width, int height, PixelFormat format, TextureFlags textureFlags, int mipCount, int arraySize, GraphicsResourceUsage usage)
        {
            if ((textureFlags & TextureFlags.UnorderedAccess) != 0)
                usage = GraphicsResourceUsage.Default;

            var desc = new TextureDescription()
            {
                Dimension = TextureDimension.Texture2D,
                Width = width,
                Height = height,
                Depth = 1,
                ArraySize = arraySize,
                Level = MSAALevel.None,
                Flags = textureFlags,
                Format = format,
                MipLevels = CalculateMipMapCount(mipCount, width, height),
                Usage = GetUsageWithFlags(usage, textureFlags),
            };
            return desc;
        }
    }
}