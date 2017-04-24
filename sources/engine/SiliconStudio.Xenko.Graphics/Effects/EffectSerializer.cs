// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    internal class EffectSerializer : DataSerializer<Effect>
    {
        public override void Serialize(ref Effect effect, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
                throw new InvalidOperationException();

            var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
            var graphicsDeviceService = services.GetSafeServiceAs<IGraphicsDeviceService>();

            var effectBytecode = stream.Read<EffectBytecode>();

            if (effect == null)
                effect = new Effect();

            effect.InitializeFrom(graphicsDeviceService.GraphicsDevice, effectBytecode);
        }
    }
}
