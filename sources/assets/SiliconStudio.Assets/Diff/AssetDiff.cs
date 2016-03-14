// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using SharpDiff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Class AssetDiff. This class cannot be inherited.
    /// </summary>
    public sealed class AssetDiff
    {
        private readonly static List<DataVisitNode> EmptyNodes = new List<DataVisitNode>();

        private readonly object baseAsset;
        private readonly object asset1;
        private readonly object asset2;
        private readonly NodeEqualityComparer equalityComparer;
        private Diff3Node computed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDiff"/> class.
        /// </summary>
        /// <param name="baseAsset">The base asset.</param>
        /// <param name="asset1">The asset1.</param>
        /// <param name="asset2">The asset2.</param>
        public AssetDiff(object baseAsset, object asset1, object asset2)
        {
            // TODO handle some null values (no asset2....etc.)
            this.baseAsset = baseAsset;
            this.asset1 = asset1;
            this.asset2 = asset2;
            this.equalityComparer = new NodeEqualityComparer(this);
            CustomVisitorsBase = new List<IDataCustomVisitor>();
            CustomVisitorsAsset1 = new List<IDataCustomVisitor>();
            CustomVisitorsAsset2 = new List<IDataCustomVisitor>();
        }

        public object BaseAsset
        {
            get
            {
                return baseAsset;
            }
        }

        public object Asset1
        {
            get
            {
                return asset1;
            }
        }

        public object Asset2
        {
            get
            {
                return asset2;
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the diff is assuming that it is in a base/child context using override informations.
        /// </summary>
        public bool UseOverrideMode { get; set; }

        /// <summary>
        /// Custom visitors that can be registered when visiting object tree.
        /// </summary>
        public List<IDataCustomVisitor> CustomVisitorsBase { get; private set; }
        public List<IDataCustomVisitor> CustomVisitorsAsset1 { get; private set; }
        public List<IDataCustomVisitor> CustomVisitorsAsset2 { get; private set; }

        public void Reset()
        {
            computed = null;
        }

        /// <summary>
        /// Computes the diff3 between <see cref="BaseAsset" />, <see cref="Asset1" /> and <see cref="Asset2" />.
        /// </summary>
        /// <param name="forceRecompute">if set to <c>true</c> force to recompute the diff.</param>
        /// <returns>The result of the diff. This result is cached so next call will return it directly.</returns>
        public Diff3Node Compute(bool forceRecompute = false)
        {
            if (computed != null && !forceRecompute)
            {
                return computed;
            }

            // If asset implement IDiffResolver, run callback
            //if (baseAsset is IDiffResolver)
            //{
            //    ((IDiffResolver)baseAsset).BeforeDiff(baseAsset, asset1, asset2);
            //}

            var baseNodes = DataVisitNodeBuilder.Run(TypeDescriptorFactory.Default, baseAsset, CustomVisitorsBase);
            var asset1Nodes = DataVisitNodeBuilder.Run(TypeDescriptorFactory.Default, asset1, CustomVisitorsAsset1);
            var asset2Nodes = DataVisitNodeBuilder.Run(TypeDescriptorFactory.Default, asset2, CustomVisitorsAsset2);
            computed =  DiffNode(baseNodes, asset1Nodes, asset2Nodes);
            return computed;
        }

        private Diff3Node DiffNode(DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node)
        {
            if (UseOverrideMode)
            {
                return DiffNodeByOverride(baseNode, asset1Node, asset2Node);
            }

            var diff3 = new Diff3Node(baseNode, asset1Node, asset2Node);

            var baseNodeDesc = GetNodeDescription(baseNode);
            var asset1NodeDesc = GetNodeDescription(asset1Node);
            var asset2NodeDesc = GetNodeDescription(asset2Node);

            if (asset1NodeDesc.Type == asset2NodeDesc.Type)
            {
                if (baseNodeDesc.Type == asset1NodeDesc.Type)
                {
                    // If all types are the same, perform a normal diff.
                    return DiffNodeWithUniformType(baseNode, asset1Node, asset2Node);
                }
                else
                {
                    // If base has a different type, but asset1 and asset2 are equal, use them. Otherwise there is a conflict with base.
                    var temp = DiffNodeWithUniformType(asset1Node, asset1Node, asset2Node);
                    diff3.ChangeType = temp.ChangeType == Diff3ChangeType.None ? Diff3ChangeType.MergeFromAsset1And2 : Diff3ChangeType.Conflict;
                    diff3.InstanceType = asset1NodeDesc.Type;
                }
            }
            else if (baseNodeDesc.Type == asset1NodeDesc.Type)
            {
                // If base and asset 1 are equal, use asset 2.
                var temp = DiffNodeWithUniformType(baseNode, asset1Node, asset1Node);
                diff3.ChangeType = temp.ChangeType == Diff3ChangeType.None ? Diff3ChangeType.MergeFromAsset2 : Diff3ChangeType.Conflict;
                diff3.InstanceType = asset2NodeDesc.Type;
            }
            else if (baseNodeDesc.Type == asset2NodeDesc.Type)
            {
                // If base and asset 2 are equal, use asset 1.
                var temp = DiffNodeWithUniformType(baseNode, asset2Node, asset2Node);
                diff3.ChangeType = temp.ChangeType == Diff3ChangeType.None ? Diff3ChangeType.MergeFromAsset1 : Diff3ChangeType.Conflict;
                diff3.InstanceType = asset1NodeDesc.Type;
            }
            else
            {
                // If one asset is unspecified, use the other.
                // If all types are different, there is a type conflict.
                if (asset1Node == null)
                {
                    diff3.ChangeType = Diff3ChangeType.MergeFromAsset2;
                    diff3.InstanceType = asset2NodeDesc.Type;
                }
                else if (asset2Node == null)
                {
                    diff3.ChangeType = Diff3ChangeType.MergeFromAsset1;
                    diff3.InstanceType = asset1NodeDesc.Type;
                }
                else
                {
                    diff3.ChangeType = Diff3ChangeType.ConflictType;
                }
            }
            return diff3;
        }

        private Diff3Node DiffNodeByOverride(DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node)
        {
            var diff3 = new Diff3Node(baseNode, asset1Node, asset2Node);

            // TODO Add merge when types are non uniform but we are in UseOverrideMode

            var baseNodeDesc = GetNodeDescription(baseNode);
            var asset1NodeDesc = GetNodeDescription(asset1Node);
            var asset2NodeDesc = GetNodeDescription(asset2Node);

            var memberBase = (diff3.BaseNode) as DataVisitMember;
            var memberAsset1 = (diff3.Asset1Node) as DataVisitMember;
            var memberAsset2 = (diff3.Asset2Node) as DataVisitMember;
            var dataVisitMember = memberBase ?? memberAsset1 ?? memberAsset2;

            // Currently, only properties/fields can have override information, so we process them separately here
            if (dataVisitMember != null)
            {
                var type = baseNodeDesc.Type ?? asset1NodeDesc.Type ?? asset2NodeDesc.Type;
                diff3.InstanceType = type;

                if (IsComparableType(dataVisitMember.HasMembers, type))
                {
                    ApplyOverrideOnValue(diff3);
                }
                else
                {
                    var baseOverride = memberBase?.Parent?.Instance?.GetOverride(memberBase.MemberDescriptor) ?? OverrideType.Base;
                    var member1Override = memberAsset1?.Parent?.Instance?.GetOverride(memberAsset1.MemberDescriptor) ?? OverrideType.Base;
                    var member2Override = memberAsset2?.Parent?.Instance?.GetOverride(memberAsset2.MemberDescriptor) ?? OverrideType.Base;

                    //            base         asset1       asset2
                    //            ----         ------       ------
                    // Type:       TB            T1           T2
                    // Override: whatever    base|whatever     (new|base)+sealed    -> If member on asset2 is sealed, or member on asset1 is base
                    // 
                    //   if T1 == T2 and TB == T1,  merge all of them
                    //   If T1 == T2
                    //       merge T1-T2
                    //   if T1 != T2
                    //       replace asset1 by asset2 value, don't perform merge on instance
                    //
                    //   In all case, replace override on asset1 by Base|Sealed

                    if (member2Override.IsSealed() || member1Override.IsBase())
                    {
                        if (asset1NodeDesc.Type == asset2NodeDesc.Type)
                        {
                            diff3 = DiffNodeWithUniformType(baseNodeDesc.Type == asset1NodeDesc.Type ? baseNode : asset2Node, asset1Node, asset2Node);
                        }
                        else
                        {
                            diff3.InstanceType = asset2NodeDesc.Type;
                            diff3.ChangeType = Diff3ChangeType.MergeFromAsset2;
                        }

                        // Force asset1 override to be base|sealed
                        diff3.FinalOverride = OverrideType.Base;
                        if (member2Override.IsSealed())
                        {
                            diff3.FinalOverride |= OverrideType.Sealed;
                        }
                    }
                    else
                    {
                        if (asset1NodeDesc.Type == asset2NodeDesc.Type)
                        {
                            diff3 = DiffNodeWithUniformType(baseNodeDesc.Type == asset1NodeDesc.Type ? baseNode : asset2Node, asset1Node, asset2Node);
                        }
                        else
                        {
                            diff3.InstanceType = asset1NodeDesc.Type;
                            diff3.ChangeType = Diff3ChangeType.MergeFromAsset1;
                        }

                        diff3.FinalOverride = member1Override;
                    }

                    // If base changed from Sealed to non-sealed, and asset1 was base|sealed, change it back to plain base.
                    if (baseOverride.IsSealed() && !member2Override.IsSealed() && member1Override == (OverrideType.Base | OverrideType.Sealed))
                    {
                        diff3.FinalOverride = OverrideType.Base;
                    }
                }
            }
            else
            {
                if (baseNodeDesc.Type != null &&
                    asset1NodeDesc.Type != null &&
                    asset2NodeDesc.Type != null)
                {
                    // cases : base  asset1  asset2
                    // case 1:  T      T       T      => Merge all instances
                    if (baseNodeDesc.Type == asset1NodeDesc.Type && baseNodeDesc.Type == asset2NodeDesc.Type)
                    {
                        // If all types are the same, perform a normal diff.
                        return DiffNodeWithUniformType(baseNode, asset1Node, asset2Node);
                    }

                    // case 2:  T      T1      T      => Only from asset1
                    if (baseNodeDesc.Type != asset1NodeDesc.Type && baseNodeDesc.Type == asset2NodeDesc.Type)
                    {
                        diff3.ChangeType = Diff3ChangeType.MergeFromAsset1;
                        diff3.InstanceType = asset1NodeDesc.Type;
                        return diff3;
                    }

                    // case 3:  T      T       T1     => Only from asset2
                    if (baseNodeDesc.Type == asset1NodeDesc.Type && baseNodeDesc.Type != asset2NodeDesc.Type)
                    {
                        diff3.ChangeType = Diff3ChangeType.MergeFromAsset2;
                        diff3.InstanceType = asset2NodeDesc.Type;
                        return diff3;
                    }
                }
                else if (baseNodeDesc.Type != null && asset1NodeDesc.Type != null && baseNodeDesc.Type == asset1NodeDesc.Type)
                {
                    // case 3:  T      T       null     => Merge from asset2 (set null on asset1)
                    diff3.ChangeType = Diff3ChangeType.MergeFromAsset2;
                    diff3.InstanceType = null;
                    return diff3;
                }

                // other cases: Always merge from asset1 (should be a conflict, but we assume that we can only use asset1
                diff3.ChangeType = Diff3ChangeType.MergeFromAsset1;
                diff3.InstanceType = asset1NodeDesc.Type;
                return diff3;
            }

            return diff3;
        }

        private Diff3Node DiffNodeWithUniformType(DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node)
        {
            var baseNodeDesc = GetNodeDescription(baseNode);
            var asset1NodeDesc = GetNodeDescription(asset1Node);
            var asset2NodeDesc = GetNodeDescription(asset2Node);

            var node = baseNode ?? asset1Node ?? asset2Node;
            var type = baseNodeDesc.Type ?? asset1NodeDesc.Type ?? asset2NodeDesc.Type;

            var diff3 = new Diff3Node(baseNode, asset1Node, asset2Node) { InstanceType = type };

            if (type == null)
            {
                // All nodes are null. This should only happen as part of a temporary diff in DiffNode()
                diff3.ChangeType = Diff3ChangeType.None;
            }
            else if (IsComparableType(node.HasMembers, type))
            {
                DiffValue(diff3, ref baseNodeDesc, ref asset1NodeDesc, ref asset2NodeDesc);
            }
            else
            {
                DiffMembers(diff3, baseNode, asset1Node, asset2Node);

                if (DictionaryDescriptor.IsDictionary(type))
                {
                    DiffDictionary(diff3, baseNode, asset1Node, asset2Node);
                }
                else if (CollectionDescriptor.IsCollection(type))
                {
                    DiffCollection(diff3, baseNode, asset1Node, asset2Node);
                }
                else if (type.IsArray)
                {
                    DiffArray(diff3, baseNode, asset1Node, asset2Node);
                }
            }

            return diff3;
        }

        private bool IsComparableType(bool hasMembers, Type type)
        {
            // A comparable type doesn't have any members, is not a collection or dictionary or array.
            bool isComparableType = ((UseOverrideMode && type.IsValueType) || !hasMembers) && !CollectionDescriptor.IsCollection(type) && !DictionaryDescriptor.IsDictionary(type) && !type.IsArray;
            return isComparableType;
        }

        private void DiffValue(Diff3Node diff3, ref NodeDescription baseNodeDesc, ref NodeDescription asset1NodeDesc, ref NodeDescription asset2NodeDesc)
        {
            var node = diff3.Asset1Node ?? diff3.Asset2Node ?? diff3.BaseNode;
            var dataVisitMember = node as DataVisitMember;
            if (dataVisitMember != null)
            {
                var diffMember = dataVisitMember.MemberDescriptor.GetCustomAttributes<DiffMemberAttribute>(true).FirstOrDefault();
                if (diffMember != null)
                {
                    if (diffMember.PreferredChange.HasValue)
                        diff3.ChangeType = diffMember.PreferredChange.Value;

                    diff3.Weight = diffMember.Weight;
                }
            }

            var instanceType = asset1NodeDesc.Instance?.GetType() ?? asset2NodeDesc.Instance?.GetType();

            object baseInstance = baseNodeDesc.Instance;
            object asset1Instance = asset1NodeDesc.Instance;
            object asset2Instance = asset2NodeDesc.Instance;

            // If this is an identifiable type (but we are for example not visiting its member), compare only the Ids instead
            if (UseOverrideMode && instanceType != null && IdentifiableHelper.IsIdentifiable(instanceType))
            {
                baseInstance = IdentifiableHelper.GetId(baseInstance);
                asset1Instance = IdentifiableHelper.GetId(asset1Instance);
                asset2Instance = IdentifiableHelper.GetId(asset2Instance);
            }

            var baseAsset1Equals = Equals(baseInstance, asset1Instance);
            var baseAsset2Equals = Equals(baseInstance, asset2Instance);
            var asset1And2Equals = Equals(asset1Instance, asset2Instance);

            diff3.ChangeType = baseAsset1Equals && baseAsset2Equals
                ? Diff3ChangeType.None
                : baseAsset2Equals ? Diff3ChangeType.MergeFromAsset1 : baseAsset1Equals ? Diff3ChangeType.MergeFromAsset2 : asset1And2Equals ? Diff3ChangeType.MergeFromAsset1And2 : Diff3ChangeType.Conflict;
        }

        private static void ApplyOverrideOnValue(Diff3Node diff3, bool isClassType = false)
        {
            var memberBase = (diff3.BaseNode) as DataVisitMember;
            var memberAsset1 = (diff3.Asset1Node) as DataVisitMember;
            var memberAsset2 = (diff3.Asset2Node) as DataVisitMember;
            var baseOverride = memberBase?.Parent?.Instance?.GetOverride(memberBase.MemberDescriptor) ?? OverrideType.Base;
            var member1Override = memberAsset1?.Parent?.Instance?.GetOverride(memberAsset1.MemberDescriptor) ?? OverrideType.Base;
            var member2Override = memberAsset2?.Parent?.Instance?.GetOverride(memberAsset2.MemberDescriptor) ?? OverrideType.Base;

            if (member2Override.IsSealed())
            {
                diff3.ChangeType = Diff3ChangeType.MergeFromAsset2;
                // Force asset1 override to be base|sealed
                diff3.FinalOverride = OverrideType.Base | OverrideType.Sealed;
            }
            else if (member1Override.IsBase())
            {
                diff3.ChangeType = Diff3ChangeType.MergeFromAsset2;
                diff3.FinalOverride = member1Override;
            }
            else
            {
                diff3.ChangeType = Diff3ChangeType.MergeFromAsset1;
                diff3.FinalOverride = member1Override;
            }

            // If base changed from Sealed to non-sealed, and asset1 was base|sealed, change it back to plain base.
            if (baseOverride.IsSealed() && !member2Override.IsSealed() && member1Override == (OverrideType.Base | OverrideType.Sealed))
            {
                diff3.FinalOverride = OverrideType.Base;
            }
        }

        private void DiffMembers(Diff3Node diff3, DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node)
        {
            var baseMembers = baseNode != null ? baseNode.Members : null;
            var asset1Members = asset1Node != null ? asset1Node.Members : null;
            var asset2Members = asset2Node != null ? asset2Node.Members : null;
            int memberCount = 0;

            if (baseMembers != null) memberCount = baseMembers.Count;
            else if (asset1Members != null) memberCount = asset1Members.Count;
            else if (asset2Members != null) memberCount = asset2Members.Count;

            for (int i = 0; i < memberCount; i++)
            {
                AddMember(diff3, DiffNode(baseMembers == null ? null : baseMembers[i],
                    asset1Members == null ? null : asset1Members[i],
                    asset2Members == null ? null : asset2Members[i]));
            }
        }

        private void DiffCollection(Diff3Node diff3, DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node)
        {
            diff3.Type = Diff3NodeType.Collection;

            var baseItems = baseNode != null ? baseNode.Items ?? EmptyNodes : EmptyNodes;
            var asset1Items = asset1Node != null ? asset1Node.Items ?? EmptyNodes : EmptyNodes;
            var asset2Items = asset2Node != null ? asset2Node.Items ?? EmptyNodes : EmptyNodes;

            var itemEqualityComparer = equalityComparer;

            var node = diff3.Asset1Node ?? diff3.Asset2Node ?? diff3.BaseNode;

            IEnumerable<Diff3Change> changes;
            bool recurseDiff = false;

            // Find an item in any of the list
            var firstItem = baseItems.FirstOrDefault(item => item.Instance != null) ?? asset1Items.FirstOrDefault(item => item.Instance != null) ?? asset2Items.FirstOrDefault(item => item.Instance != null);

            // For now, in the context of UseOverrideMode and we have identifiers per item, use DiffCollectionByIds instead
            if (UseOverrideMode && firstItem != null)
            {
                if (IdentifiableHelper.IsIdentifiable(firstItem.Instance.GetType()))
                {
                    DiffCollectionByIds(diff3, baseNode, asset1Node, asset2Node);
                    return;
                }
                else if (firstItem.Instance is Guid)
                {
                    DiffCollectionByGuids(diff3, baseNode, asset1Node, asset2Node);
                    return;
                }
            }

            // If we have a DiffUseAsset1Attribute, list of Asset1Node becomes authoritative.
            var dataVisitMember = node as DataVisitMember;
            var diffMemberAttribute = dataVisitMember != null ? dataVisitMember.MemberDescriptor.GetCustomAttributes<DiffMemberAttribute>(true).FirstOrDefault() : null;
            if (diffMemberAttribute != null && diffMemberAttribute.PreferredChange.HasValue)
            {
                diff3.Weight = diffMemberAttribute.Weight;
            }

            if (diffMemberAttribute != null && diffMemberAttribute.PreferredChange.HasValue)
            {
                var diffChange = diffMemberAttribute.PreferredChange.Value == Diff3ChangeType.MergeFromAsset2
                    ? new Diff3Change { ChangeType = SharpDiff.Diff3ChangeType.MergeFrom2, From2 = new Span(0, asset2Items.Count - 1) }
                    : new Diff3Change { ChangeType = SharpDiff.Diff3ChangeType.MergeFrom1, From1 = new Span(0, asset1Items.Count - 1) };

                changes = new[] { diffChange };
                // TODO: Try to merge back data of matching nodes
            }
            else if (firstItem != null && typeof(IDiffKey).IsAssignableFrom(firstItem.InstanceType))
            {
                // If item implement IDataDiffKey, we will use that as equality key
                changes = Diff3.Compare(
                    baseItems.Select(x => ((IDiffKey)x.Instance).GetDiffKey()).ToList(),
                    asset1Items.Select(x => ((IDiffKey)x.Instance).GetDiffKey()).ToList(),
                    asset2Items.Select(x => ((IDiffKey)x.Instance).GetDiffKey()).ToList());
                recurseDiff = true;
            }
            else
            {
                // Otherwise, do a full node comparison
                itemEqualityComparer.Reset();
                changes = Diff3.Compare(baseItems, asset1Items, asset2Items, itemEqualityComparer);
            }

            foreach (var change in changes)
            {
                switch (change.ChangeType)
                {
                    case SharpDiff.Diff3ChangeType.Equal:
                        for (int i = 0; i < change.Base.Length; i++)
                        {
                            var diff3Node = recurseDiff
                                ? DiffNode(baseItems[change.Base.From + i], asset1Items[change.From1.From + i], asset2Items[change.From2.From + i])
                                : new Diff3Node(baseItems[change.Base.From + i], asset1Items[change.From1.From + i], asset2Items[change.From2.From + i]) { ChangeType = Diff3ChangeType.None };
                            AddItem(diff3, diff3Node, change.From1.From != 0);
                        }
                        break;

                    case SharpDiff.Diff3ChangeType.MergeFrom1:
                        for (int i = 0; i < change.From1.Length; i++)
                        {
                            var diff3Node = new Diff3Node(null, asset1Items[change.From1.From + i], null) { ChangeType = Diff3ChangeType.MergeFromAsset1 };
                            AddItem(diff3, diff3Node, change.From1.From != 0);
                        }
                        break;

                    case SharpDiff.Diff3ChangeType.MergeFrom2:
                        for (int i = 0; i < change.From2.Length; i++)
                        {
                            var diff3Node = new Diff3Node(null, null, asset2Items[change.From2.From + i]) { ChangeType = Diff3ChangeType.MergeFromAsset2 };
                            AddItem(diff3, diff3Node, true);
                        }
                        break;

                    case SharpDiff.Diff3ChangeType.MergeFrom1And2:
                        for (int i = 0; i < change.From2.Length; i++)
                        {
                            var diff3Node = recurseDiff
                                ? DiffNode(null, asset1Items[change.From1.From + i], asset2Items[change.From2.From + i])
                                : new Diff3Node(null, asset1Items[change.From1.From + i], asset2Items[change.From2.From + i]) { ChangeType = Diff3ChangeType.MergeFromAsset1And2 };
                            AddItem(diff3, diff3Node, change.From1.From != 0);
                        }
                        break;

                    case SharpDiff.Diff3ChangeType.Conflict:
                        int baseIndex = change.Base.IsValid ? change.Base.From : -1;
                        int from1Index = change.From1.IsValid ? change.From1.From : -1;
                        int from2Index = change.From2.IsValid ? change.From2.From : -1;

                        // If there are changes only from 1 or 2 or base.Length == list1.Length == list2.Length, then try to make a diff per item
                        // else output the conflict as a full conflict
                        bool tryResolveConflict = false;
                        if (baseIndex >= 0)
                        {
                            if (from1Index >= 0 && from2Index >= 0)
                            {
                                if ((change.Base.Length == change.From1.Length && change.Base.Length == change.From2.Length)
                                    || (change.From1.Length == change.From2.Length))
                                {
                                    tryResolveConflict = true;
                                }
                            }
                            else if (from1Index >= 0)
                            {
                                tryResolveConflict = change.Base.Length == change.From1.Length;
                            }
                            else if (from2Index >= 0)
                            {
                                tryResolveConflict = change.Base.Length == change.From2.Length;
                            }
                            else
                            {
                                tryResolveConflict = true;
                            }
                        }

                        // Iterate on items
                        while ((baseIndex >= 0 && baseItems.Count > 0) || (from1Index >= 0 && asset1Items.Count > 0) || (from2Index >= 0 && asset2Items.Count > 0))
                        {
                            var baseItem = GetSafeFromList(baseItems, ref baseIndex, ref change.Base);
                            var asset1Item = GetSafeFromList(asset1Items, ref from1Index, ref change.From1);
                            var asset2Item = GetSafeFromList(asset2Items, ref from2Index, ref change.From2);

                            var diff3Node = tryResolveConflict || recurseDiff ? 
                                DiffNode(baseItem, asset1Item, asset2Item) :
                                new Diff3Node(baseItem, asset1Item, asset2Item) { ChangeType = Diff3ChangeType.Conflict };
                            AddItem(diff3, diff3Node, true);
                        }
                        break;
                }
            }

            // Any missing item? (we can detect this only at the end)
            var newItemCount = diff3.Items != null ? diff3.Items.Count : 0;
            if (asset1Items.Count != newItemCount)
            {
                diff3.ChangeType = Diff3ChangeType.Children;
            }
        }

        private void DiffCollectionByIds(Diff3Node diff3, DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node)
        {
            DiffCollectionByIdsGeneric(diff3, baseNode, asset1Node, asset2Node, IdentifiableHelper.GetId, DiffNode);
        }

        private Guid GetSafeGuidForCollectionItem(object instance, int index, Func<object, Guid> idGetter)
        {
            // If the instance is null, we still need a guid, so we are generating one based on the index from the collection
            // If null values is put at the same index for base/asset1/asset2, they will be matching
            // If not a merge my not generate optimal merge with nulls
            // In general, null items in collections that maybe mergeable should be avoided
            if (instance == null)
            {
                return new Guid(index, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            }
            return idGetter(instance);
        }

        private void DiffCollectionByIdsGeneric(Diff3Node diff3, DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node, Func<object, Guid> idGetter, Func<DataVisitNode, DataVisitNode, DataVisitNode, Diff3Node> diff3Getter)
        {
            var baseItems = baseNode != null ? baseNode.Items ?? EmptyNodes : EmptyNodes;
            var asset1Items = asset1Node != null ? asset1Node.Items ?? EmptyNodes : EmptyNodes;
            var asset2Items = asset2Node != null ? asset2Node.Items ?? EmptyNodes : EmptyNodes;

            // Pre-build dictionary
            var items = new Dictionary<Guid, Diff3CollectionByIdItem>();

            for (int i = 0; i < baseItems.Count; i++)
            {
                var item = baseItems[i];
                var id = GetSafeGuidForCollectionItem(item.Instance, i, idGetter);
                Diff3CollectionByIdItem entry;
                items.TryGetValue(id, out entry);
                entry.BaseIndex = i;
                items[id] = entry;
            }
            for (int i = 0; i < asset1Items.Count; i++)
            {
                var item = asset1Items[i];
                var id = GetSafeGuidForCollectionItem(item.Instance, i, idGetter);
                Diff3CollectionByIdItem entry;
                items.TryGetValue(id, out entry);
                entry.Asset1Index = i;
                items[id] = entry;
            }
            for (int i = 0; i < asset2Items.Count; i++)
            {
                var item = asset2Items[i];
                var id = GetSafeGuidForCollectionItem(item.Instance, i, idGetter);
                Diff3CollectionByIdItem entry;
                items.TryGetValue(id, out entry);
                entry.Asset2Index = i;
                items[id] = entry;
            }

            foreach (var idAndEntry in items.OrderBy(item => item.Value.Asset1Index ?? item.Value.Asset2Index ?? item.Value.BaseIndex ?? 0))
            {
                var entry = idAndEntry.Value;

                bool hasBase = entry.BaseIndex.HasValue;
                bool hasAsset1 = entry.Asset1Index.HasValue;
                bool hasAsset2 = entry.Asset2Index.HasValue;

                int baseIndex = entry.BaseIndex ?? -1;
                int asset1Index = entry.Asset1Index ?? -1;
                int asset2Index = entry.Asset2Index ?? -1;

                if (hasBase && hasAsset1 && hasAsset2)
                {
                    // If asset was already in base and is still valid for asset1 and asset2
                    var diff3Node = diff3Getter(baseNode.Items[baseIndex], asset1Node.Items[asset1Index], asset2Node.Items[asset2Index]);
                    // Use index from asset2 if baseIndex = asset1Index, else use from asset1 index
                    AddItemByPosition(diff3, diff3Node, baseIndex == asset1Index ? asset2Index : asset1Index, true);
                }
                else if (!hasBase)
                {
                    // If no base, there is at least either asset1 or asset2 but not both, as they can't have the same id
                    if (hasAsset1)
                    {
                        var diff3Node = new Diff3Node(null, asset1Node.Items[asset1Index], null) { ChangeType = Diff3ChangeType.MergeFromAsset1, InstanceType = asset1Node.Items[asset1Index].InstanceType };
                        AddItemByPosition(diff3, diff3Node, asset1Index, true);
                    }
                    else if (hasAsset2)
                    {
                        var diff3Node = new Diff3Node(null, null, asset2Node.Items[asset2Index]) { ChangeType = Diff3ChangeType.MergeFromAsset2, InstanceType = asset2Node.Items[asset2Index].InstanceType };
                        AddItemByPosition(diff3, diff3Node, asset2Index, true);
                    }
                }
                else
                {
                    // either item was removed from asset1 or asset2, so we assume that it is completely removed 
                    // (not strictly correct, because one item could change a sub-property, but we assume that when a top-level list item is removed, we remove it, even if it has changed internally)
                }
            }

            // The diff will only applied to children (we don't support members)
            diff3.ChangeType = Diff3ChangeType.Children;

            // Cleanup any hole in the list
            // If new items are found, just cleanup
            if (diff3.Items != null)
            {
                // Because in the previous loop, we can add some hole while trying to merge index (null nodes), we need to remove them from here.
                for (int i = diff3.Items.Count - 1; i >= 0; i--)
                {
                    if (diff3.Items[i] == null)
                    {
                        diff3.Items.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Diff a collection of Guid (supposed to be unique)
        /// </summary>
        /// <param name="diff3"></param>
        /// <param name="baseNode"></param>
        /// <param name="asset1Node"></param>
        /// <param name="asset2Node"></param>
        private void DiffCollectionByGuids(Diff3Node diff3, DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node)
        {
            DiffCollectionByIdsGeneric(diff3, baseNode, asset1Node, asset2Node, o => (Guid)o, (a1, a2, a3) => new Diff3Node(a1, a2, a3) { InstanceType = typeof(Guid), ChangeType = Diff3ChangeType.MergeFromAsset1 });
        }

        private static DataVisitNode GetSafeFromList(List<DataVisitNode> nodes, ref int index, ref Span span)
        {
            if (nodes == null || index < 0) return null;
            if (index >= nodes.Count || (span.IsValid && index > span.To))
            {
                index = -1;
                return null;
            }
            var value = nodes[index];
            index++;
            if (index >= nodes.Count || (span.IsValid && index > span.To)) index = -1;
            return value;
        }

        private void DiffDictionary(Diff3Node diff3, DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node)
        {
            diff3.Type = Diff3NodeType.Dictionary;

            var baseItems = baseNode != null ? baseNode.Items : null;
            var asset1Items = asset1Node != null ? asset1Node.Items : null;
            var asset2Items = asset2Node != null ? asset2Node.Items : null;

            // Build dictionary: key => base, v1, v2
            var keyNodes = new Dictionary<object, Diff3DictionaryItem>();
            Diff3DictionaryItem diff3Item;
            if (baseItems != null)
            {
                foreach (var dataVisitNode in baseItems.OfType<DataVisitDictionaryItem>())
                {
                    keyNodes.Add(dataVisitNode.Key, new Diff3DictionaryItem() { Base = dataVisitNode });
                }
            }
            if (asset1Items != null)
            {
                foreach (var dataVisitNode in asset1Items.OfType<DataVisitDictionaryItem>())
                {
                    keyNodes.TryGetValue(dataVisitNode.Key, out diff3Item);
                    diff3Item.Asset1 = dataVisitNode;
                    keyNodes[dataVisitNode.Key] = diff3Item;
                }
            }
            if (asset2Items != null)
            {
                foreach (var dataVisitNode in asset2Items.OfType<DataVisitDictionaryItem>())
                {
                    keyNodes.TryGetValue(dataVisitNode.Key, out diff3Item);
                    diff3Item.Asset2 = dataVisitNode;
                    keyNodes[dataVisitNode.Key] = diff3Item;
                }
            }

            // Perform merge on dictionary
            foreach (var keyNode in keyNodes)
            {
                var valueNode = keyNode.Value;

                Diff3Node diffValue;

                //  base     v1      v2     action
                //  ----     --      --     ------
                //   a        b       c     Diff(a,b,c)
                //  null      b       c     Diff(null, b, c)
                if (valueNode.Asset1 != null && valueNode.Asset2 != null)
                {
                    diffValue = DiffNode(valueNode.Base, valueNode.Asset1, valueNode.Asset2);
                }
                else if (valueNode.Asset1 == null)
                {
                    //   a       null     c     MergeFrom1 (unchanged)
                    //  null     null     c     MergeFrom2
                    //   a       null    null   MergeFrom1 (unchanged)
                    diffValue = new Diff3Node(valueNode.Base, null, valueNode.Asset2)
                    {
                        ChangeType = valueNode.Base == null ? Diff3ChangeType.MergeFromAsset2 : Diff3ChangeType.MergeFromAsset1,
                    };
                }
                else
                {
                    //   a        a      null   MergeFrom2 (removed)
                    //   a        b      null   Conflict
                    //  null      b      null   MergeFrom1 (unchanged)
                    var changeType = Diff3ChangeType.MergeFromAsset1;
                    if (valueNode.Base != null)
                    {
                        var diffNode = DiffNode(valueNode.Base, valueNode.Asset1, valueNode.Base);
                        changeType = diffNode.FindDifferences().Any()
                            ? Diff3ChangeType.Conflict
                            : Diff3ChangeType.MergeFromAsset2;
                    }

                    diffValue = new Diff3Node(valueNode.Base, valueNode.Asset1, null) { ChangeType = changeType };
                }

                AddItem(diff3, diffValue);
            }
        }

        private void DiffArray(Diff3Node diff3, DataVisitNode baseNode, DataVisitNode asset1Node, DataVisitNode asset2Node)
        {
            var baseItems = baseNode != null ? baseNode.Items : null;
            var asset1Items = asset1Node != null ? asset1Node.Items : null;
            var asset2Items = asset2Node != null ? asset2Node.Items : null;
            int itemCount = -1;

            if (baseItems != null)
            {
                itemCount = baseItems.Count;
            }

            if (asset1Items != null)
            {
                var newLength = asset1Items.Count;
                if (itemCount >= 0 && itemCount != newLength)
                {
                    diff3.ChangeType = Diff3ChangeType.ConflictArraySize;
                    return;
                }
                itemCount = newLength;
            }

            if (asset2Items != null)
            {
                var newLength = asset2Items.Count;
                if (itemCount >= 0 && itemCount != newLength)
                {
                    diff3.ChangeType = Diff3ChangeType.ConflictArraySize;
                    return;
                }
                itemCount = newLength;
            }

            for (int i = 0; i < itemCount; i++)
            {
                AddItem(diff3, DiffNode(baseItems == null ? null : baseItems[i],
                    asset1Items == null ? null : asset1Items[i],
                    asset2Items == null ? null : asset2Items[i]));
            }

            // TODO: Add diff by ids on array
        }

        /// <summary>
        /// Adds a member to this instance.
        /// </summary>
        /// <param name="thisObject">The this object.</param>
        /// <param name="member">The member.</param>
        /// <exception cref="System.ArgumentNullException">member</exception>
        private static void AddMember(Diff3Node thisObject, Diff3Node member)
        {
            if (member == null) throw new ArgumentNullException("member");
            if (thisObject.Members == null)
                thisObject.Members = new List<Diff3Node>();

            member.Parent = thisObject;
            if (member.ChangeType != Diff3ChangeType.None)
            {
                thisObject.ChangeType = Diff3ChangeType.Children;
            }
            thisObject.Members.Add(member);
        }

        /// <summary>
        /// Adds an item (array, list or dictionary item) to this instance.
        /// </summary>
        /// <param name="thisObject">The this object.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="System.ArgumentNullException">item</exception>
        private static void AddItem(Diff3Node thisObject, Diff3Node item, bool hasChildrenChanged = false)
        {
            if (thisObject.Items == null)
                thisObject.Items = new List<Diff3Node>();

            // Add at the end of the list
            AddItemByPosition(thisObject, item, thisObject.Items.Count, hasChildrenChanged);
        }

        private static void AddItemByPosition(Diff3Node thisObject, Diff3Node item, int position, bool hasChildrenChanged = false)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (thisObject.Items == null)
                thisObject.Items = new List<Diff3Node>();

            item.Parent = thisObject;
            if (item.ChangeType != Diff3ChangeType.None || hasChildrenChanged)
            {
                thisObject.ChangeType = Diff3ChangeType.Children;
            }

            if (position >= thisObject.Items.Count)
            {
                int count = (position - thisObject.Items.Count + 1);
                for (int i = 0; i < count; i++)
                {
                    thisObject.Items.Add(null);
                }
            }

            if (thisObject.Items[position] == null)
            {
                item.Index = position;
                thisObject.Items[position] = item;
            }
            else
            {
                item.Index = position + 1;
                thisObject.Items.Insert(item.Index, item);
            }
        }

        private NodeDescription GetNodeDescription(DataVisitNode node)
        {
            if (node == null)
            {
                return new NodeDescription();
            }

            var instanceType = node.InstanceType;
            if (NullableDescriptor.IsNullable(instanceType))
            {
                instanceType = Nullable.GetUnderlyingType(instanceType);
            }

            return new NodeDescription(node.Instance, instanceType);
        }

        private struct NodeDescription
        {
            public NodeDescription(object instance, Type type)
            {
                Instance = instance;
                Type = type;
            }

            public readonly object Instance;

            public readonly Type Type;
        }

        struct Diff3CollectionByIdItem
        {
            public int? BaseIndex;
            public int? Asset1Index;
            public int? Asset2Index;
        }

        private struct Diff3DictionaryItem
        {
            public DataVisitDictionaryItem Base;

            public DataVisitDictionaryItem Asset1;

            public DataVisitDictionaryItem Asset2;
        }

        private class NodeEqualityComparer : IEqualityComparer<DataVisitNode>
        {
            private Dictionary<KeyComparison, bool> equalityCache = new Dictionary<KeyComparison, bool>();
            private AssetDiff diffManager;

            public NodeEqualityComparer(AssetDiff diffManager)
            {
                if (diffManager == null) throw new ArgumentNullException("diffManager");
                this.diffManager = diffManager;
            }

            public void Reset()
            {
                equalityCache.Clear();
            }

            public bool Equals(DataVisitNode x, DataVisitNode y)
            {
                var key = new KeyComparison(x, y);
                bool result;
                if (equalityCache.TryGetValue(key, out result))
                {
                    return result;
                }

                var diff3 = diffManager.DiffNode(x, y, x);

                result = !diff3.FindDifferences().Any();
                equalityCache.Add(key, result);
                return result;
            }

            public int GetHashCode(DataVisitNode obj)
            {
                int hashCode = 0;

                foreach (var node in obj.Children(x => true))
                {
                    if (node.HasItems)
                        hashCode = hashCode * 17 + node.Items.Count;
                    else if (node.HasMembers)
                        hashCode = hashCode * 11 + node.Members.Count;
                    else if (diffManager.IsComparableType(false, node.InstanceType) && node.InstanceType.IsPrimitive && node.Instance != null) // Ignore non-primitive types, to be safe (GetHashCode doesn't do deep comparison)
                        hashCode = hashCode * 13 + node.Instance.GetHashCode();
                }

                return hashCode;
            }

            private struct KeyComparison : IEquatable<KeyComparison>
            {
                public KeyComparison(DataVisitNode node1, DataVisitNode node2)
                {
                    Node1 = node1;
                    Node2 = node2;
                }

                public readonly DataVisitNode Node1;

                public readonly DataVisitNode Node2;


                public bool Equals(KeyComparison other)
                {
                    return ReferenceEquals(Node1, other.Node1) && ReferenceEquals(Node2, other.Node2);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    return obj is KeyComparison && Equals((KeyComparison)obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        return ((Node1 != null ? Node1.GetHashCode() : 0) * 397) ^ (Node2 != null ? Node2.GetHashCode() : 0);
                    }
                }

                public static bool operator ==(KeyComparison left, KeyComparison right)
                {
                    return left.Equals(right);
                }

                public static bool operator !=(KeyComparison left, KeyComparison right)
                {
                    return !left.Equals(right);
                }
            }
        }
    }
}