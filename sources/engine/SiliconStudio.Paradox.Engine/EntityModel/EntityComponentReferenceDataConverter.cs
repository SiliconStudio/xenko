// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.EntityModel
{
    internal class EntityComponentReferenceDataConverter<TSource> : DataConverter<EntityComponentReference<TSource>, TSource>
        where TSource : EntityComponent
    {
        public override bool CacheResult
        {
            get { return true; }
        }

        public override void ConvertFromData(ConverterContext converterContext, EntityComponentReference<TSource> componentReference, ref TSource component)
        {
            // Convert component
            component = converterContext.ConvertFromData<TSource>(componentReference.Value);
        }

        public override void ConvertToData(ConverterContext converterContext, ref EntityComponentReference<TSource> componentReference, TSource component)
        {
            throw new NotImplementedException();
        }
    }
}