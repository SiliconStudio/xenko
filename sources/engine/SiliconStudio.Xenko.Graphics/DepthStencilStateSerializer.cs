// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Serializer for <see cref="DepthStencilState"/>.
    /// </summary>
    [DataSerializerGlobal(typeof(DepthStencilStateSerializer))]
    public class DepthStencilStateSerializer : DataSerializer<DepthStencilState>
    {
        public override void Serialize(ref DepthStencilState depthStencilState, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                var depthStencilStateDescription = depthStencilState.Description;
                stream.Serialize(ref depthStencilStateDescription, mode);
            }
            else
            {
                // If we have a graphics context, we will instantiate GPU state, otherwise a CPU one.
                var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                var graphicsDeviceService = services != null ? services.GetSafeServiceAs<IGraphicsDeviceService>() : null;

                var depthStencilStateDescription = DepthStencilStateDescription.Default;
                stream.Serialize(ref depthStencilStateDescription, mode);
                depthStencilState = graphicsDeviceService != null
                    ? DepthStencilState.New(graphicsDeviceService.GraphicsDevice, depthStencilStateDescription)
                    : DepthStencilState.NewFake(depthStencilStateDescription);
            }
        }
    }
}