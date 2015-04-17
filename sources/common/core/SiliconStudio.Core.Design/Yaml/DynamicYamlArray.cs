using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SharpYaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Dynamic version of <see cref="YamlSequenceNode"/>.
    /// </summary>
    public class DynamicYamlArray : DynamicYamlObject, IEnumerable
    {
        internal YamlSequenceNode node;

        public YamlSequenceNode Node
        {
            get
            {
                return node;
            }
        }

        public DynamicYamlArray(YamlSequenceNode node)
        {
            this.node = node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return node.Children.Select(ConvertToDynamic).ToArray().GetEnumerator();
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type.IsAssignableFrom(node.GetType()))
            {
                result = node;
            }
            else
            {
                throw new InvalidOperationException();
            }
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            var key = Convert.ToInt32(indexes[0]);
            node.Children[key] = ConvertFromDynamic(value);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var key = Convert.ToInt32(indexes[0]);
            result = ConvertToDynamic(node.Children[key]);
            return true;
        }

        public void Add(object value)
        {
            node.Children.Add(ConvertFromDynamic(value));
        }
    }
}