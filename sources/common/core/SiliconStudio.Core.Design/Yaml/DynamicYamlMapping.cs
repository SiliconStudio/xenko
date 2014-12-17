using System;
using System.Dynamic;
using SharpYaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Dynamic version of <see cref="YamlMappingNode"/>.
    /// </summary>
    public class DynamicYamlMapping : DynamicYamlObject
    {
        internal YamlMappingNode node;

        public YamlMappingNode Node
        {
            get
            {
                return node;
            }
        }

        public DynamicYamlMapping(YamlMappingNode node)
        {
            this.node = node;
        }

        public void MoveChild(object key, int movePosition)
        {
            var yamlKey = ConvertFromDynamic(key);
            var keyPosition = node.Children.IndexOf(yamlKey);

            if (keyPosition == movePosition)
                return;

            // Remove child
            var item = node.Children[keyPosition];
            node.Children.RemoveAt(keyPosition);

            // Adjust insertion position (if we insert in a position after the removal position)
            if (movePosition > keyPosition)
                movePosition--;

            // Insert item at new position
            node.Children.Insert(movePosition, item.Key, item.Value);
        }

        public void RemoveChild(object key)
        {
            var yamlKey = ConvertFromDynamic(key);
            var keyPosition = node.Children.IndexOf(yamlKey);
            node.Children.RemoveAt(keyPosition);
        }

        public int IndexOf(object key)
        {
            var yamlKey = ConvertFromDynamic(key);

            return node.Children.IndexOf(yamlKey);
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

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetValue(new YamlScalarNode(binder.Name));
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var key = new YamlScalarNode(binder.Name);

            if (value is DynamicYamlEmpty)
                node.Children.Remove(key);
            else
                node.Children[key] = ConvertFromDynamic(value);
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            var key = ConvertFromDynamic(indexes[0]);
            if (value is DynamicYamlEmpty)
                node.Children.Remove(key);
            else
                node.Children[key] = ConvertFromDynamic(value);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var key = ConvertFromDynamic(indexes[0]);
            result = GetValue(key);
            return true;
        }

        private object GetValue(YamlNode key)
        {
            YamlNode result;
            if (node.Children.TryGetValue(key, out result))
            {
                return ConvertToDynamic(result);
            }
            return null;
        }
    }
}