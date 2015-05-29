// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.ProceduralModels
{
    internal class ProceduralModelDescriptorContentSerializer : ContentSerializerBase<Model>
    {
        private static readonly DataContentSerializerHelper<ProceduralModelDescriptor> DataSerializerHelper = new DataContentSerializerHelper<ProceduralModelDescriptor>();

        public override Type SerializationType
        {
            get { return typeof(ProceduralModelDescriptor); }
        }

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, Model model)
        {
            var proceduralModel = new ProceduralModelDescriptor();
            DataSerializerHelper.Serialize(context, stream, proceduralModel);

            var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);

            proceduralModel.GenerateModel(services, model);
        }

    }
}