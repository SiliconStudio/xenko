// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Core.Serialization.Converters
{
    public sealed class ConverterContext
    {
        static List<DataConverter> converters = new List<DataConverter>();
        static Dictionary<ConvertersBySourceTypeKey, DataConverter> convertersBySourceType = new Dictionary<ConvertersBySourceTypeKey, DataConverter>();

        private Dictionary<ConvertedObjectKey, object> convertedObjects = new Dictionary<ConvertedObjectKey, object>();

        public PropertyContainer Tags;

        public static void RegisterConverter<TData, TSource>(DataConverter<TData, TSource> converter)
        {
            converters.Add(converter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TData ConvertToData<TData>(object source)
        {
            TData result = default(TData);
            ConvertToData(ref result, source);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ConvertFromData<TSource>(object data, ConvertFromDataFlags flags = ConvertFromDataFlags.Default)
        {
            TSource result = default(TSource);
            ConvertFromData(data, ref result, flags);
            return result;
        }

        public void ConvertFromData<TSource>(object data, ref TSource source, ConvertFromDataFlags flags = ConvertFromDataFlags.Default)
        {
            // Special case: null
            if (data == null)
            {
                source = default(TSource);
                return;
            }

            var dataType = data.GetType();
            var dataIsValueType = dataType.GetTypeInfo().IsValueType;

            var dataConverter = GetDataConverter(dataType, typeof(TSource), ConversionType.DataToObject);
            if (dataConverter == null)
            {
                // Special case: no conversion required
                if (data is TSource)
                {
                    source = (TSource)data;
                    return;
                }

                throw new InvalidOperationException(string.Format("Could not find a valid converter from type {0} to {1}", dataType, typeof(TSource)));
            }

            object sourceObject;
            bool alreadyStoredInConvertedObjects = ((flags & ConvertFromDataFlags.Construct) == 0);

            if (!dataIsValueType && dataConverter.CacheResult && ((flags & ConvertFromDataFlags.Construct) != 0))
            {
                // Check cache
                object cachedSource;
                if (convertedObjects.TryGetValue(new ConvertedObjectKey(data, dataConverter.ObjectType), out cachedSource))
                {
                    source = (TSource)cachedSource;
                    return;
                }

                sourceObject = source;

                if (dataConverter.CanConstruct)
                {
                    dataConverter.ConstructFromData(this, data, ref sourceObject);

                    // Register information so that if RegisterCurrentObject is called, it will be stored in convertedObjects
                    if (sourceObject != null)
                    {
                        convertedObjects.Add(new ConvertedObjectKey(data, dataConverter.ObjectType), sourceObject);
                        alreadyStoredInConvertedObjects = true;
                    }
                }
            }
            else
            {
                sourceObject = source;
            }

            if (((flags & ConvertFromDataFlags.Convert) != 0))
            {
                dataConverter.ConvertFromData(this, data, ref sourceObject);

                if (!dataIsValueType && dataConverter.CacheResult)
                {
                    // If not stored manually through RegisterCurrentObject, register our object now
                    // Note: we can'ty reuse objectKey as it might have been reentrant
                    if (!alreadyStoredInConvertedObjects)
                        convertedObjects.Add(new ConvertedObjectKey(data, dataConverter.ObjectType), sourceObject);
                }
            }

            source = (TSource)sourceObject;
        }

        public void ConvertToData<TData>(ref TData data, object source)
        {
            // Special case: null
            if (source == null)
            {
                data = default(TData);
                return;
            }

            var sourceType = source.GetType();
            var sourceIsValueType = sourceType.GetTypeInfo().IsValueType;

            var dataConverter = GetDataConverter(typeof(TData), sourceType, ConversionType.ObjectToData);
            if (dataConverter == null)
            {
                // Special case: no conversion required
                if (source is TData)
                {
                    data = (TData)source;
                    return;
                }

                throw new InvalidOperationException(string.Format("Could not find a valid converter from type {0} to {1}", sourceType, typeof(TData)));
            }

            if (!sourceIsValueType && dataConverter.CacheResult)
            {
                // Check cache
                object cachedData;
                if (convertedObjects.TryGetValue(new ConvertedObjectKey(source, dataConverter.ObjectType), out cachedData))
                {
                    data = (TData)cachedData;
                    return;
                }
            } 

            object dataObject = data;
            dataConverter.ConvertToData(this, ref dataObject, source);

            if (!sourceIsValueType && dataConverter.CacheResult)
            {
                convertedObjects.Add(new ConvertedObjectKey(source, dataConverter.ObjectType), dataObject);
            }

            data = (TData)dataObject;
        }

        public static DataConverter GetDataConverter(Type dataType, Type objectType, ConversionType conversionType)
        {
            lock (convertersBySourceType)
            {
                DataConverter dataConverter;
                if (!convertersBySourceType.TryGetValue(new ConvertersBySourceTypeKey(dataType, objectType), out dataConverter))
                {
                    // 1. Find in registered converters
                    foreach (var existingConverter in converters)
                    {
                        bool matching = conversionType == ConversionType.ObjectToData
                            ? objectType == existingConverter.ObjectType && dataType.GetTypeInfo().IsAssignableFrom(existingConverter.DataType.GetTypeInfo())
                            : dataType == existingConverter.DataType && objectType.GetTypeInfo().IsAssignableFrom(existingConverter.ObjectType.GetTypeInfo());
                        if (matching)
                        {
                            if (dataConverter != null)
                                throw new InvalidOperationException(string.Format("Found two matching converters between types {0} and {1}", dataType, objectType));
                            dataConverter = existingConverter;
                        }
                    }

                    // 2. Try to resolve usual collections: List<>, Dictionary<,>, etc...
                    if (dataConverter == null)
                    {
                        DataConverterResult objectResult, dataResult;
                        if (GetDataConverter(dataType, objectType, typeof(IList<>), conversionType, out dataResult, out objectResult))
                        {
                            dataConverter = (DataConverter)Activator.CreateInstance(typeof(ListDataConverter<,,,>).MakeGenericType(dataResult.Type, objectResult.Type, dataResult.InterfaceType.GenericTypeArguments[0], objectResult.InterfaceType.GenericTypeArguments[0]));
                        }
                        else if (GetDataConverter(dataType, objectType, typeof(IDictionary<,>), conversionType, out dataResult, out objectResult))
                        {
                            dataConverter = (DataConverter)Activator.CreateInstance(
                                typeof(DictionaryDataConverter<,,,,,>).MakeGenericType(
                                    dataResult.Type, objectResult.Type,
                                    dataResult.InterfaceType.GenericTypeArguments[0], dataResult.InterfaceType.GenericTypeArguments[1],
                                    objectResult.InterfaceType.GenericTypeArguments[0], objectResult.InterfaceType.GenericTypeArguments[1]));
                        }
                    }

                    // 3. Try to resolve ContentReference<>
                    if (dataConverter == null)
                    {
                        DataConverterResult dataResult;
                        if (GetGenericTypeArgumentsForInterface(dataType, typeof(ContentReference<>), out dataResult))
                        {
                            var contentDataType = dataResult.InterfaceType.GenericTypeArguments[0];
                            var contentObjectType = objectType;
                            var itemDataConverter = GetDataConverter(dataType.GenericTypeArguments[0], objectType, conversionType);

                            // Check if there is a way to convert (use type in this case), otherwise transfer as is
                            if (conversionType == ConversionType.DataToObject)
                            {
                                contentObjectType = (itemDataConverter != null) ? itemDataConverter.ObjectType : contentDataType;
                            }

                            dataConverter = (DataConverter)Activator.CreateInstance(typeof(ContentReferenceDataConverter<,>).MakeGenericType(contentDataType, contentObjectType));
                        }
                    }

                    convertersBySourceType.Add(new ConvertersBySourceTypeKey(dataType, objectType), dataConverter);
                }
                return dataConverter;
            }
        }

        private static bool GetGenericTypeArgumentsForInterface(Type type, Type genericType, out DataConverterResult result)
        {
            result = new DataConverterResult();

            var typeInfo = type.GetTypeInfo();
            var genericTypeInfo = genericType.GetTypeInfo();

            // Check the type itself
            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == genericType)
            {
                result.InterfaceType = type;
                result.Type = type;
                return true;
            }

            // If interface, let's check its implemented interfaces as well
            if (genericTypeInfo.IsInterface)
            {
                foreach (var @interface in typeInfo.ImplementedInterfaces)
                {
                    var interfaceTypeInfo = @interface.GetTypeInfo();
                    if (interfaceTypeInfo.IsGenericType && interfaceTypeInfo.GetGenericTypeDefinition() == genericType)
                    {
                        result.InterfaceType = interfaceTypeInfo.AsType();
                        result.Type = type;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool GetDataConverter(Type dataType, Type objectType, Type genericType, ConversionType conversionType, out DataConverterResult dataResult, out DataConverterResult objectResult)
        {
            bool objectResultFound = GetGenericTypeArgumentsForInterface(objectType, genericType, out objectResult);
            bool dataResultFound = GetGenericTypeArgumentsForInterface(dataType, genericType, out dataResult);

            switch (conversionType)
            {
                case ConversionType.ObjectToData:
                    if (objectResultFound && !dataResultFound)
                    {
                        dataResultFound = true;
                        var genericTypeArguments = objectResult.InterfaceType.GenericTypeArguments.Select(x =>
                        {
                            var dataConverter = GetDataConverter(typeof(object), x, conversionType);
                            return dataConverter != null ? dataConverter.DataType : x;
                        });
                        dataResult.Type = dataResult.InterfaceType = genericType.MakeGenericType(genericTypeArguments.ToArray());
                    }
                    break;
                case ConversionType.DataToObject:
                    if (dataResultFound && !objectResultFound)
                    {
                        objectResultFound = true;
                        var genericTypeArguments = dataResult.InterfaceType.GenericTypeArguments.Select(x =>
                        {
                            var dataConverter = GetDataConverter(x, typeof(object), conversionType);
                            return dataConverter != null ? dataConverter.ObjectType : x;
                        });
                        objectResult.Type = objectResult.InterfaceType = genericType.MakeGenericType(genericTypeArguments.ToArray());
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("conversionType");
            }

            return (objectResultFound && dataResultFound);
        }
        
        public enum ConversionType
        {
            ObjectToData,
            DataToObject,
        }

        struct DataConverterResult
        {
            /// <summary>
            /// Real type.
            /// </summary>
            public Type Type;

            /// <summary>
            /// Implemented interface.
            /// </summary>
            public Type InterfaceType;
        }

        struct ConvertersBySourceTypeKey : IEquatable<ConvertersBySourceTypeKey>
        {
            public Type DataType;
            public Type ObjectType;

            public ConvertersBySourceTypeKey(Type dataType, Type objectType)
            {
                DataType = dataType;
                ObjectType = objectType;
            }

            public bool Equals(ConvertersBySourceTypeKey other)
            {
                return DataType.Equals(other.DataType) && ObjectType.Equals(other.ObjectType);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ConvertersBySourceTypeKey && Equals((ConvertersBySourceTypeKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (DataType.GetHashCode()*397) ^ ObjectType.GetHashCode();
                }
            }
        }

        struct ConvertedObjectKey : IEquatable<ConvertedObjectKey>
        {
            public object Object;
            public Type ObjectType;

            public ConvertedObjectKey(object o, Type objectType)
            {
                Object = o;
                ObjectType = objectType;
            }

            public bool Equals(ConvertedObjectKey other)
            {
                return Object.Equals(other.Object) && ObjectType.Equals(other.ObjectType);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ConvertedObjectKey && Equals((ConvertedObjectKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Object.GetHashCode()*397) ^ ObjectType.GetHashCode();
                }
            }
        }
    }
}