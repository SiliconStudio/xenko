// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SharpYaml.Serialization;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Dynamic version of <see cref="YamlMappingNode"/>.
    /// </summary>
    public class DynamicYamlMapping : DynamicYamlObject, IDynamicYamlNode, IEnumerable
    {
        internal YamlMappingNode node { get; set; }

        private Dictionary<string, string> nodeMapping;

        private Dictionary<string, OverrideType> overrides;

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
            ParseOverrides();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return node.Children.Select(x => new KeyValuePair<dynamic, dynamic>(ConvertToDynamic(x.Key), ConvertToDynamic(x.Value))).ToArray().GetEnumerator();
        }


        private YamlNode ConvertFromDynamicForKey(object value)
        {
            if (value is string)
            {
                value = GetRealPropertyName((string)value);
            }
            return ConvertFromDynamic(value);
        }

        public void AddChild(object key, object value)
        {
            var yamlKey = ConvertFromDynamicForKey(key);
            var yamlValue = ConvertFromDynamic(value);

            var keyPosition = node.Children.IndexOf(yamlKey);
            if (keyPosition != -1)
                return;

            node.Children.Add(yamlKey, yamlValue);
        }

        public void MoveChild(object key, int movePosition)
        {
            var yamlKey = ConvertFromDynamicForKey(key);
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
            var yamlKey = ConvertFromDynamicForKey(key);
            var keyPosition = node.Children.IndexOf(yamlKey);
            if (keyPosition != -1)
                node.Children.RemoveAt(keyPosition);
        }

        public int IndexOf(object key)
        {
            var yamlKey = ConvertFromDynamicForKey(key);

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
            if (node.Children.TryGetValue(new YamlScalarNode(GetRealPropertyName(binder.Name)), out tempNode))
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
            var key = new YamlScalarNode(GetRealPropertyName(binder.Name));

            if (value is DynamicYamlEmpty)
                node.Children.Remove(key);
            else
                node.Children[key] = ConvertFromDynamic(value);
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            var key = ConvertFromDynamicForKey(indexes[0]);
            if (value is DynamicYamlEmpty)
                node.Children.Remove(key);
            else
                node.Children[key] = ConvertFromDynamic(value);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var key = ConvertFromDynamicForKey(indexes[0]);
            result = GetValue(key);
            return true;
        }

        public void SetOverride(string member, OverrideType type)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            YamlNode previousMemberKey = null;
            YamlNode previousMemberValue = null;

            if (nodeMapping == null)
            {
                nodeMapping = new Dictionary<string, string>();
            }
            else
            {
                string previousMemberName;
                if (nodeMapping.TryGetValue(member, out previousMemberName))
                {
                    previousMemberKey = new YamlScalarNode(previousMemberName);
                    node.Children.TryGetValue(previousMemberKey, out previousMemberValue);
                }
                nodeMapping.Remove(member);
            }

            if (overrides == null)
            {
                overrides = new Dictionary<string, OverrideType>();
            }
            else
            {
                overrides.Remove(member);
            }

            string newMemberName = member;
            if (type != OverrideType.Base)
            {
                newMemberName = $"{member}{type.ToText()}";
            }
            nodeMapping[member] = newMemberName;
            overrides[member] = type;

            // Remap the original YAML node with the override type
            if (previousMemberKey != null)
            {
                int index = node.Children.IndexOf(previousMemberKey);
                node.Children.RemoveAt(index);
                node.Children.Insert(index, new YamlScalarNode(newMemberName), previousMemberValue);
            }
        }

        public OverrideType GetOverride(string key)
        {
            if (overrides == null)
            {
                return OverrideType.Base;
            }
            OverrideType type;
            if (overrides.TryGetValue(key, out type))
            {
                return type;
            }
            return OverrideType.Base;
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

        private string GetRealPropertyName(string name)
        {
            if (nodeMapping == null)
            {
                return name;
            }

            string realPropertyName;
            if (nodeMapping.TryGetValue(name, out realPropertyName))
            {
                return realPropertyName;
            }
            return name;
        }

        /// <summary>
        /// This method will extract overrides information and maintain a separate dictionary to ensure mapping between
        /// a full property name without override (MyProperty) and with its override (e.g: MyProperty! for sealed MyProperty)
        /// </summary>
        private void ParseOverrides()
        {
            foreach (var keyValue in node)
            {
                var scalar = keyValue.Key as YamlScalarNode;
                if (scalar?.Value != null)
                {
                    var isPostFixNew = scalar.Value.EndsWith(Override.PostFixNew);
                    var isPostFixSealed = scalar.Value.EndsWith(Override.PostFixSealed);
                    if (isPostFixNew || isPostFixSealed)
                    {
                        var name = scalar.Value;
                        var type = isPostFixNew ? OverrideType.New : OverrideType.Sealed;

                        var isPostFixNewSealedAlt = name.EndsWith(Override.PostFixNewSealedAlt);
                        var isPostFixNewSealed = name.EndsWith(Override.PostFixNewSealed);
                        if (isPostFixNewSealed || isPostFixNewSealedAlt)
                        {
                            type = OverrideType.New | OverrideType.Sealed;
                            name = name.Substring(0, name.Length - 2);
                        }
                        else
                        {
                            name = name.Substring(0, name.Length - 1);
                        }
                        if (nodeMapping == null)
                        {
                            nodeMapping = new Dictionary<string, string>();
                        }

                        nodeMapping[name] = scalar.Value;

                        if (overrides == null)
                        {
                            overrides = new Dictionary<string, OverrideType>();
                        }
                        overrides[name] = type;
                    }
                }
            }
        }

    }
}