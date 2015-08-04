// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering
{
    // Need to add support for fields in auto data converter
    [DataContract]
    public class MeshDraw
    {
        public PrimitiveType PrimitiveType;

        public int DrawCount;

        public int StartLocation;

        public VertexBufferBinding[] VertexBuffers;

        public IndexBufferBinding IndexBuffer;
    }
}