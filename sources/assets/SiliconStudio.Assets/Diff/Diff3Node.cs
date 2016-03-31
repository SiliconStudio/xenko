// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Text;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Diff
{
    public class Diff3Node : IDataVisitNode<Diff3Node>
    {
        private static readonly Func<Diff3Node, bool> StaticCheckVisitChildrenWithWeights = CheckVisitChildrenWithWeights;
        private static readonly Func<Diff3Node, bool> StaticCheckVisitChildren = CheckVisitChildren;
        private static readonly Func<Diff3Node, bool> StaticCheckVisitNode = CheckVisitNode;
        private static readonly Func<Diff3Node, bool> StaticCheckVisitLeaf = CheckVisitLeaf;

        public Diff3Node()
        {
        }

        public Diff3Node(DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node)
        {
            BaseNode = baseNode;
            Asset1Node = asset1Node;
            Asset2Node = asset2Node;
        }

        public DataVisitNode BaseNode { get; set; }

        public DataVisitNode Asset1Node { get; set; }

        public DataVisitNode Asset2Node { get; set; }

        public Diff3ChangeType ChangeType { get; set; }

        public OverrideType? FinalOverride { get; set; }

        public float Weight { get; set; }

        public Diff3Node Parent { get; set; }

        public int Index { get; set; }

        public Diff3NodeType Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the instance. Null if instance type is different between the nodes.
        /// </summary>
        /// <value>The type of the instance.</value>
        public Type InstanceType { get; set; }

        public IEnumerable<Diff3Node> FindDifferences()
        {
            return this.Children(StaticCheckVisitNode, StaticCheckVisitChildren);
        }

        public IEnumerable<Diff3Node> FindDifferencesWithWeights()
        {
            return this.Children(StaticCheckVisitNode, StaticCheckVisitChildrenWithWeights);
        }

        public IEnumerable<Diff3Node> FindLeafDifferences()
        {
            return this.Children(StaticCheckVisitLeaf, StaticCheckVisitChildren);
        }

        private static bool CheckVisitChildren(Diff3Node diff3)
        {
            // Returns true if the node has some changes or if it has some children
            return diff3.ChangeType != Diff3ChangeType.None;
        }

        private static bool CheckVisitChildrenWithWeights(Diff3Node diff3)
        {
            return CheckVisitChildren(diff3) || diff3.Weight != 0;
        }

        private static bool CheckVisitNode(Diff3Node diff3)
        {
            return true;
        }

        private static bool CheckVisitLeaf(Diff3Node diff3)
        {
            return diff3.ChangeType != Diff3ChangeType.Children;
        }

        public bool HasConflict
        {
            get
            {
                return ChangeType >= Diff3ChangeType.Conflict;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has <see cref="Members"/>.
        /// </summary>
        /// <value><c>true</c> if this instance has members; otherwise, <c>false</c>.</value>
        public bool HasMembers
        {
            get
            {
                return Members != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has <see cref="Items"/>.
        /// </summary>
        /// <value><c>true</c> if this instance has items; otherwise, <c>false</c>.</value>
        public bool HasItems
        {
            get
            {
                return Items != null;
            }
        }

        public List<Diff3Node> Members { get; set; }

        public List<Diff3Node> Items { get; set; }

        /// <summary>
        /// Applies the override calculated by the merge for the current member.
        /// </summary>
        public void ApplyOverride()
        {
            if (!FinalOverride.HasValue)
            {
                return;
            }

            var member = Asset1Node as DataVisitMember;
            member?.Parent?.Instance?.SetOverride(member.MemberDescriptor, FinalOverride.Value);
        }

        /// <summary>
        /// Replace the value for the asset1 for this data node.
        /// </summary>
        /// <param name="dataInstance">The data instance.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="isRemoved"></param>
        public void ReplaceValue(object dataInstance, Func<Diff3Node, DataVisitNode> selector)
        {
            if (selector == null) throw new ArgumentNullException("selector");
            var node = this.Asset1Node ?? this.BaseNode ?? this.Asset2Node;

            var dataNode = selector(this);
            var parentNode = Parent;

            if (node is DataVisitMember)
            {
                ((DataVisitMember)dataNode).SetValue(dataInstance);
            }
            else if (node is DataVisitListItem)
            {
                var listItem= (DataVisitListItem)node;
                var descriptor = listItem.Descriptor;
                var parentVisitNode = selector(parentNode);

                var parentInstance = parentVisitNode.Instance;
                descriptor.SetValue(parentInstance, Index, dataInstance);

                // Update parent in case the member is a struct
                var member = parentVisitNode as DataVisitMember;
                if (member != null)
                {
                    member.UpdateInstance();
                }
            }
            else if (node is DataVisitDictionaryItem)
            {
                var dictItem = (DataVisitDictionaryItem)node;

                var descriptor = dictItem.Descriptor;
                var parentVisitNode = selector(parentNode);
                var parentInstance = parentVisitNode.Instance;
                if (dataInstance == null)
                {
                    descriptor.Remove(parentInstance, dictItem.Key);
                }
                else
                {
                    descriptor.SetValue(parentInstance, dictItem.Key, dataInstance);
                }

                // Update parent in case the member is a struct
                var member = parentVisitNode as DataVisitMember;
                if (member != null)
                {
                    member.UpdateInstance();
                }
            }
            else if (node is DataVisitArrayItem)
            {
                var arrayItem = (DataVisitArrayItem)node;
                ((Array)selector(parentNode).Instance).SetValue(dataInstance, arrayItem.Index);
            }
        }


        public override string ToString()
        {
            var text = new StringBuilder();

            var node = this.Asset1Node ?? this.BaseNode ?? this.Asset2Node;
            if (node is DataVisitMember)
                text.AppendFormat("{0}: ", ((DataVisitMember)node).MemberDescriptor.Name);

            text.Append("Diff = ");
            text.Append(ChangeType);
            if (HasMembers)
            {
                text.AppendFormat(" Members = {0}", Members.Count);
            }
            if (HasItems)
            {
                text.AppendFormat(" Items = {0}", Items.Count);
            }

            text.AppendFormat(" Type = {0}", InstanceType == null ? "unknown" : InstanceType.FullName);

            return text.ToString();
        }
    }
}
