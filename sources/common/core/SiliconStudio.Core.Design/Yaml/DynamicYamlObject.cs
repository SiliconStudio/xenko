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

            if (obj is YamlNode)
                return (YamlNode)obj;

            if (obj is DynamicYamlMapping)
                return ((DynamicYamlMapping)obj).node;
            if (obj is DynamicYamlArray)
                return ((DynamicYamlArray)obj).node;
            if (obj is DynamicYamlScalar)
                return ((DynamicYamlScalar)obj).node;

            return new YamlScalarNode(String.Format(CultureInfo.InvariantCulture, "{0}", obj));
        }

        protected static object ConvertToDynamic(object obj)
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