// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Serializer for <see cref="SamplerState"/>.
    /// </summary>
    [DataSerializerGlobal(typeof(SamplerStateSerializer))]
    public class SamplerStateSerializer : DataSerializer<SamplerState>
    {
        public override void Serialize(ref SamplerState samplerState, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                var samplerStateDescription = samplerState.Description;
                stream.Serialize(ref samplerStateDescription, mode);
            }
            else
            {
                // If we have a graphics context, we will instantiate GPU state, otherwise a CPU one.
                var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                var graphicsDeviceService = services != null ? services.GetSafeServiceAs<IGraphicsDeviceService>() : null;

                var samplerStateDescription = SamplerStateDescription.Default;
                stream.Serialize(ref samplerStateDescription, mode);
                samplerState = graphicsDeviceService != null
                    ? SamplerState.New(graphicsDeviceService.GraphicsDevice, samplerStateDescription)
                    : SamplerState.NewFake(samplerStateDescription);
            }
        }
    }
}
