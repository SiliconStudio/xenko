using System;
using System.Dynamic;
using SharpYaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Dynamic version of <see cref="YamlScalarNode"/>.
    /// </summary>
    public class DynamicYamlScalar : DynamicYamlObject
    {
        internal YamlScalarNode node;

        public YamlScalarNode Node
        {
            get
            {
                return node;
            }
        }

        public DynamicYamlScalar(YamlScalarNode node)
        {
            this.node = node;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = Convert.ChangeType(node.Value, binder.Type);
            return true;
        }
    }
}