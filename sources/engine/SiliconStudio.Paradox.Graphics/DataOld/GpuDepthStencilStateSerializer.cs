// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics.DataOld
{
    public class GpuDepthStencilStateSerializer : ContentSerializerBase<DepthStencilState>
    {
        public GraphicsDevice graphicsDevice;

        public GpuDepthStencilStateSerializer(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, DepthStencilState depthStencilState)
        {
            if (context.Mode == ArchiveMode.Deserialize)
            {
                var depthStencilStateDescription = default(DepthStencilStateDescription);
                stream.Serialize(ref depthStencilStateDescription, context.Mode);
                depthStencilState = DepthStencilState.New(graphicsDevice, depthStencilStateDescription);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override object Construct(ContentSerializerContext context)
        {
            return null;
        }
    }
}
