// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;

using SharpYaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    public static class DynamicYamlExtensions
    {
        public static T ConvertTo<T>(IDynamicYamlNode yamObject)
        {
            using (var memoryStream = new MemoryStream())
            {
                // convert Yaml nodes to string
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    var yamlStream = new YamlStream { new YamlDocument(yamObject.Node) };
                    yamlStream.Save(streamWriter, true, YamlSerializer.GetSerializerSettings().PreferredIndent);

                    streamWriter.Flush();
                    memoryStream.Position = 0;

                    // convert string to object
                    return (T)YamlSerializer.Deserialize(memoryStream, typeof(T), null);
                }
            }
        }

        public static IDynamicYamlNode ConvertFrom<T>(T dataObject)
        {
            using (var stream = new MemoryStream())
            {
                // convert data to string
                YamlSerializer.Serialize(stream, dataObject);

                stream.Position = 0;

                // convert string to Yaml nodes
                using (var reader = new StreamReader(stream))
                {
                    var yamlStream = new YamlStream();
                    yamlStream.Load(reader);
                    return (IDynamicYamlNode)DynamicYamlObject.ConvertToDynamic(yamlStream.Documents[0].RootNode);
                }
            }
        }
    }
}