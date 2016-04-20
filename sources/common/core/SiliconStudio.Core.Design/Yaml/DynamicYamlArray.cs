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
    /// Dynamic version of <see cref="YamlSequenceNode"/>.
    /// </summary>
    public class DynamicYamlArray : DynamicYamlObject, IDynamicYamlNode, IEnumerable
    {
        internal YamlSequenceNode node;

        public YamlSequenceNode Node
        {
            get
            {
                return node;
            }
        }

        YamlNode IDynamicYamlNode.Node => Node;

        public DynamicYamlArray(YamlSequenceNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
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

        public void RemoveAt(int index)
        {
            node.Children.RemoveAt(index);
        }
    }
}