// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Core.Serialization.Contents
{
    public class DataContentSerializerHelper
    {
        internal static DataSerializerFactory DefaultSerializerFactory = DataSerializerFactory.CreateDataSerializerFactory("Default");
    }
    public class DataContentSerializerHelper<T> : DataContentSerializerHelper
    {
        private DataSerializer<T> dataSerializer;

        public void Serialize(ContentSerializerContext context, SerializationStream stream, T obj)
        {
            // Get serializer
            // Note: Currently registered serializer is the content reference one
            // However, we would like to serialize the actual type here
            if (dataSerializer == null)
            {
                if (!DataContentSerializerHelper.DefaultSerializerFactory.CanSerialize(typeof(T)))
                    throw new InvalidOperationException(string.Format("Could not find a serializer for type {0}", typeof(T)));
                dataSerializer = (DataSerializer<T>)DataContentSerializerHelper.DefaultSerializerFactory.GetSerializer(typeof(T)).Value;
                if (dataSerializer is IDataSerializerInitializer)
                    ((IDataSerializerInitializer)dataSerializer).Initialize(stream.Context.SerializerSelector);
            }

            // Serialize object
            stream.SerializeExtended(ref obj, context.Mode, dataSerializer);
        }
    }

    /// <summary>
    /// ContentSerializer that simply defers serialization to low level serialization.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    public class DataContentSerializer<T> : ContentSerializerBase<T>
    {
        private DataContentSerializerHelper<T> dataSerializerHelper = new DataContentSerializerHelper<T>();

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, T obj)
        {
            dataSerializerHelper.Serialize(context, stream, obj);
        }
    }
}
