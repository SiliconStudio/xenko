// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SharpYaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Dynamic version of <see cref="YamlMappingNode"/>.
    /// </summary>
    public class DynamicYamlMapping : DynamicYamlObject, IDynamicYamlNode, IEnumerable
    {
        internal YamlMappingNode node;

        public YamlMappingNode Node
        {
            get
            {
                return node;
            }
        }

        YamlNode IDynamicYamlNode.Node => Node;

        public DynamicYamlMapping(YamlMappingNode node)
        {
            this.node = node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return node.Children.Select(x => new KeyValuePair<dynamic, dynamic>(ConvertToDynamic(x.Key), ConvertToDynamic(x.Value))).ToArray().GetEnumerator();
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
            if (keyPosition != -1)
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
            YamlNode tempNode;
            if (node.Children.TryGetValue(new YamlScalarNode(binder.Name), out tempNode))
            {
                result = ConvertToDynamic(tempNode);
                return true;
            }
            result = null;
            // Probably not very good, but unfortunately we have some asset upgraders that relies on null check to check existence
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