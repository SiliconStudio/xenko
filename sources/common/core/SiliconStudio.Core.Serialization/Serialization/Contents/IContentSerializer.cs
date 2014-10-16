// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Core.Serialization.Contents
{
    public interface IContentSerializer
    {
        Type SerializationType { get; }

        Type ActualType { get; }

        //void PreloadReferences(ContentSerializerContext executor, List<PackageObjectReference> references);

        //bool CanSerializeContext(ContentSerializerContext executor);

        //void Serialize(ContentSerializerExecutorBase executor, ref object obj/*, ref object intermediateData*/);
        void Serialize(ContentSerializerContext context, SerializationStream stream, ref object obj);

        object Construct(ContentSerializerContext context);
    }

    public interface IContentSerializer<T> : IContentSerializer
    {
        void Serialize(ContentSerializerContext context, SerializationStream stream, ref T obj);
    }
}
