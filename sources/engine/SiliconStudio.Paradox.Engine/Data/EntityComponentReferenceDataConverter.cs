// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.EntityModel.Data;

namespace SiliconStudio.Paradox.Data
{
    public class EntityComponentReferenceDataConverter<TSource> : DataConverter<EntityComponentReference<TSource>, TSource>
        where TSource : EntityComponent
    {
        public override bool CacheResult
        {
            get { return true; }
        }

        public override void ConvertFromData(ConverterContext converterContext, EntityComponentReference<TSource> componentReference, ref TSource component)
        {
            // Load entity
            var entity = converterContext.ConvertFromData<Entity>(componentReference.Entity);

            // Get its component
            component = entity.Get(componentReference.Component);
        }

        public override void ConvertToData(ConverterContext converterContext, ref EntityComponentReference<TSource> componentReference, TSource component)
        {
            ContentReference<EntityData> entityReference = null;
            converterContext.ConvertToData(ref entityReference, component.Entity);

            // Find key of this component
            PropertyKey<TSource> componentKey = null;
            if (component.Entity == null)
                throw new InvalidOperationException("Entity of a referenced component can't be null");

            foreach (var entityComponent in component.Entity.Tags)
            {
                if (entityComponent.Value is EntityComponent
                    && entityComponent.Value == component)
                {
                    componentKey = (PropertyKey<TSource>)entityComponent.Key;
                }
            }

            if (componentKey == null)
                throw new InvalidOperationException("Could not find the component in its entity");

            componentReference = new EntityComponentReference<TSource>(entityReference, componentKey);
        }
    }
}