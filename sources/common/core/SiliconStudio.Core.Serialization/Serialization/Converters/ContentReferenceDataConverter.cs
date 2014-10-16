// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Core.Serialization.Converters
{
    public class ContentReferenceDataConverter<TData, TSource> : DataConverter<ContentReference<TData>, TSource> where TData : class
    {
        public override bool CacheResult
        {
            get { return false; }
        }

        public override void ConvertToData(ConverterContext converterContext, ref ContentReference<TData> data, TSource obj)
        {
            var contentSerializerContext = converterContext.Tags.Get(ContentSerializerContext.ContentSerializerContextProperty);

            // TODO: When to stop conversion and switch to Location?
            if (contentSerializerContext != null)
            {
                data = new ContentReference<TData>() { Value = converterContext.ConvertToData<TData>(obj) };
            }
            else
            {
                data = new ContentReference<TData>() { Value = converterContext.ConvertToData<TData>(obj) };
            }
        }

        public override void ConvertFromData(ConverterContext converterContext, ContentReference<TData> data, ref TSource source)
        {
            var contentSerializerContext = converterContext.Tags.Get(ContentSerializerContext.ContentSerializerContextProperty);

            // TODO: Load through AssetManager if not loaded yet.
            if (contentSerializerContext != null)
            {
                // Not loaded yet?
                var parentAssetReference = contentSerializerContext.AssetReference;
                AssetManager.AssetReference assetReference;
                var value = contentSerializerContext.AssetManager.DeserializeObject(parentAssetReference, out assetReference, data.Location, typeof(TSource), AssetManagerLoaderSettings.IgnoreReferences, converterContext);

                source = converterContext.ConvertFromData<TSource>(value);
            }
            else
            {
                source = converterContext.ConvertFromData<TSource>(data.Value);
            }
        }
    }
}