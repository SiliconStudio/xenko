// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;

namespace SiliconStudio.BuildEngine
{
    public class ParadoxDataContractOperationBehavior : DataContractSerializerOperationBehavior
    {
        private static ParadoxXmlObjectSerializer serializer = new ParadoxXmlObjectSerializer();

        public ParadoxDataContractOperationBehavior(OperationDescription operation)
            : base(operation)
        {
        }

        public ParadoxDataContractOperationBehavior(
            OperationDescription operation,
            DataContractFormatAttribute dataContractFormatAttribute)
            : base(operation, dataContractFormatAttribute)
        {
        }

        public override XmlObjectSerializer CreateSerializer(
            Type type, string name, string ns, IList<Type> knownTypes)
        {
            return serializer;
        }

        public override XmlObjectSerializer CreateSerializer(
            Type type, XmlDictionaryString name, XmlDictionaryString ns,
            IList<Type> knownTypes)
        {
            return serializer;
        }
    }
}