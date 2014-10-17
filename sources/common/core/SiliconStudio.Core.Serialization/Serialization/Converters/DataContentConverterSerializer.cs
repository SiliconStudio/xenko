// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Core.Serialization.Converters
{
    public class DataContentConverterSerializer<TSource> : ContentSerializerBase<TSource>
    {
        private DataConverter converter;

        public override Type SerializationType
        {
            get { return converter.DataType; }
        }

        public DataContentConverterSerializer()
            : this(typeof(object))
        {
            
        }

        protected DataContentConverterSerializer(Type dataType)
        {
            // For now, get data type from converter
            // Not sure if it will be good enough for everything.
            // Another option would be to do it when data types are auto-generated.
            converter = ConverterContext.GetDataConverter(dataType, typeof(TSource), ConverterContext.ConversionType.ObjectToData);
            if (converter == null)
                throw new InvalidOperationException(string.Format("Could not find a valid converter for type {0}", typeof(TSource)));
        }

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, ref TSource obj)
        {
            // Serialize object
            if (context.Mode == ArchiveMode.Deserialize)
            {
                AssetManager.AssetReference assetReference;

                // Recursively call deserialization of this stream as another type.
                // We use special DeserializeObjectRecursive instead of DeserializeObject to avoid reopening this stream.
                var dataObject = context.AssetManager.DeserializeObjectRecursive(null, out assetReference, context.Url, converter.DataType, AssetManagerLoaderSettings.IgnoreReferences, context, stream.NativeStream, SerializationType);

                object source = null;

                // Transfer Context information to the Converter
                ConverterContext converterContext = context.ConverterContext;
                if (converterContext == null)
                {
                    // First time: create context, and register current object
                    converterContext = new ConverterContext { Tags = stream.Context.Tags };
                    context.ConverterContext = converterContext;
                }

                // Pre-construct object (if available)
                converterContext.ConvertFromData(dataObject, ref source, ConvertFromDataFlags.Construct);

                // If object could be constructed, register it so that it can properly be referenced
                if (source != null)
                    context.AssetManager.SetAssetObject(context.AssetReference, source);

                // Actually convert object
                converterContext.ConvertFromData(dataObject, ref source, ConvertFromDataFlags.Convert);

                obj = (TSource)source;

                // Unload data object (not necessary anymore)
                context.AssetManager.Unload(dataObject);
            }
            else
            {
                // Transfer Context information to the Converter
                object dataObject = null;
                converter.ConvertToData(new ConverterContext { Tags = stream.Context.Tags }, ref dataObject, obj);

                MemberNonSealedSerializer.SerializeExtended(stream, converter.DataType, ref dataObject, context.Mode);
            }
        }
    }

    public class DataContentConverterSerializer<TData, TSource> : DataContentConverterSerializer<TSource>
    {
        public DataContentConverterSerializer()
            : base(typeof(TData))
        {
        }
    }
}