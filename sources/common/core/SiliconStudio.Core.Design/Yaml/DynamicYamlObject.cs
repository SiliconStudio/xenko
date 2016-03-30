// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Dynamic;
using System.Globalization;
using SharpYaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    public abstract class DynamicYamlObject : DynamicObject
    {
        protected static YamlNode ConvertFromDynamic(object obj)
        {
            if (obj == null)
                return new YamlScalarNode("null");

            if (obj is string)
            {
                return new YamlScalarNode((string)obj);
            }

            if (obj is YamlNode)
                return (YamlNode)obj;

            if (obj is DynamicYamlMapping)
                return ((DynamicYamlMapping)obj).Node;
            if (obj is DynamicYamlArray)
                return ((DynamicYamlArray)obj).node;
            if (obj is DynamicYamlScalar)
                return ((DynamicYamlScalar)obj).node;

            if (obj is bool)
                return new YamlScalarNode((bool)obj ? "true" : "false");

            return new YamlScalarNode(string.Format(CultureInfo.InvariantCulture, "{0}", obj));
        }

        public static object ConvertToDynamic(object obj)
        {
            if (obj is YamlScalarNode)
                return new DynamicYamlScalar((YamlScalarNode)obj);
            if (obj is YamlMappingNode)
                return new DynamicYamlMapping((YamlMappingNode)obj);
            if (obj is YamlSequenceNode)
                return new DynamicYamlArray((YamlSequenceNode)obj);

            return obj;

        }
    }
}