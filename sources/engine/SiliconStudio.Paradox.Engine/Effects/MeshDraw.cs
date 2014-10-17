// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    // Need to add support for fields in auto data converter
    [DataConverter(AutoGenerate = true)]
    public class MeshDraw
    {
        [DataMemberConvert]
        public PrimitiveType PrimitiveType;

        [DataMemberConvert]
        public int DrawCount;

        [DataMemberConvert]
        public int StartLocation;

        [DataMemberConvert]
        public VertexBufferBinding[] VertexBuffers;

        [DataMemberConvert]
        public IndexBufferBinding IndexBuffer;
    }
}