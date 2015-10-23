using SharpYaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    public interface IDynamicYamlNode
    {
        YamlNode Node { get; }
    }
}