// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;

namespace SiliconStudio.BuildEngine
{
    public class XenkoDataContractOperationBehavior : DataContractSerializerOperationBehavior
    {
        private static XenkoXmlObjectSerializer serializer = new XenkoXmlObjectSerializer();

        public XenkoDataContractOperationBehavior(OperationDescription operation)
            : base(operation)
        {
        }

        public XenkoDataContractOperationBehavior(
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
