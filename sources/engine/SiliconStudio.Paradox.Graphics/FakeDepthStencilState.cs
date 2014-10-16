// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Graphics
{
    public class FakeDepthStencilState : DepthStencilState
    {
        public FakeDepthStencilState()
        {
        }

        public FakeDepthStencilState(DepthStencilStateDescription description) : base(description)
        {
        }
    }

    public class DepthStencilStateSerializer : SiliconStudio.Core.Serialization.Contents.ContentSerializerBase<DepthStencilState>
    {
        public override void Serialize(SiliconStudio.Core.Serialization.Contents.ContentSerializerContext context, SerializationStream stream, ref DepthStencilState depthStencilState)
        {
            if (context.Mode == ArchiveMode.Serialize)
            {
                var depthStencilStateDescription = depthStencilState.Description;
                stream.Serialize(ref depthStencilStateDescription, context.Mode);
            }
            else
            {
                var depthStencilStateDescription = DepthStencilStateDescription.Default;
                stream.Serialize(ref depthStencilStateDescription, context.Mode);
                depthStencilState = new FakeDepthStencilState(depthStencilStateDescription);
            }
        }

        public override object Construct(SiliconStudio.Core.Serialization.Contents.ContentSerializerContext context)
        {
            return null;
        }
    }
}
