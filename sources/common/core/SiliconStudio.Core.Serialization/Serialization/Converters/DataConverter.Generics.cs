// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Serialization.Converters
{
    /// <summary>
    /// Typed Converter class to/from a given data type.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="T">The type of the source.</typeparam>
    public abstract class DataConverter<TData, T> : DataConverter
    {
        /// <inheritdoc/>
        public override Type DataType
        {
            get { return typeof(TData); }
        }

        /// <inheritdoc/>
        public override Type ObjectType
        {
            get { return typeof(T); }
        }


        public virtual void ConstructFromData(ConverterContext converterContext, TData data, ref T obj)
        {
        }

        /// <inheritdoc/>
        public override void ConstructFromData(ConverterContext converterContext, object data, ref object obj)
        {
            var dataT = (TData)data;
            var objT = (T)obj;
            ConstructFromData(converterContext, dataT, ref objT);
            obj = objT;
        }


        public abstract void ConvertFromData(ConverterContext converterContext, TData data, ref T obj);

        /// <inheritdoc/>
        public override void ConvertFromData(ConverterContext converterContext, object data, ref object obj)
        {
            var dataT = (TData)data;
            var objT = (T)obj;
            ConvertFromData(converterContext, dataT, ref objT);
            obj = objT;
        }

        /// <summary>
        /// Converts the given source object to its data counterpart.
        /// </summary>
        /// <param name="converterContext">The converter context.</param>
        /// <param name="data">The data.</param>
        /// <param name="obj"></param>
        public abstract void ConvertToData(ConverterContext converterContext, ref TData data, T obj);

        /// <inheritdoc/>
        public override void ConvertToData(ConverterContext converterContext, ref object data, object obj)
        {
            var dataT = (TData)data;
            var objT = (T)obj;
            ConvertToData(converterContext, ref dataT, objT);
            data = dataT;
        }
    }
}