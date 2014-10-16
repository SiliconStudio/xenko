using System;
using System.Collections.Generic;
using System.Linq;

using SharpDiff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Diff
{
    public class DataMatcher
    {
        private readonly ModelNodeComparer modelNodeComparer;
        private readonly ModelNodeCanAlign modelNodeCanAlign;
        private readonly ModelNodeSimilarityComparer modelNodeSimilarityComparer;
        private readonly Dictionary<NodeKey, DataMatch> matches = new Dictionary<NodeKey, DataMatch>();
        private readonly TypeDescriptorFactory descriptorFactory;
        
        public DataMatcher(TypeDescriptorFactory descriptorFactory)
        {
            if (descriptorFactory == null) throw new ArgumentNullException("descriptorFactory");
            this.descriptorFactory = descriptorFactory;
            modelNodeComparer = new ModelNodeComparer(this);
            modelNodeSimilarityComparer = new ModelNodeSimilarityComparer(this);
            modelNodeCanAlign = new ModelNodeCanAlign(modelNodeSimilarityComparer);
        }

        public DataMatch Match(DataVisitNode node1, DataVisitNode node2)
        {
            DataMatch match;
            var key = new NodeKey(node1, node2);
            if (matches.TryGetValue(key, out match))
            {
                return match;
            }


            match = MatchInternal(node1, node2);
            Console.WriteLine("Match {0} : {1} vs {2}", node1, node2, match);
            matches[key] = match;

            return match;
        }

        private DataMatch MatchInternal(DataVisitNode node1, DataVisitNode node2)
        {
            // Check if nodes are null
            if ((ReferenceEquals(node1, null) || ReferenceEquals(node2, null) || node1.GetType() != node2.GetType()))
            {
                return node1 == node2 ? DataMatch.Empty : UnMatched(node1, node2);
            }

            var match = new DataMatch();

            if (node1 is DataVisitRootNode)
            {
                if (node1.Instance == null || node2.Instance == null || node1.Instance.GetType() != node2.Instance.GetType())
                {
                    return ReferenceEquals(node1.Instance, node2.Instance) ? DataMatch.MatchOne : UnMatched(node1, node2);
                }
            }
            else if (node1 is DataVisitMember)
            {
                match += MatchValue(node1, ((DataVisitMember)node1).Value, node2, ((DataVisitMember)node2).Value);
            }
            else if (node1 is DataVisitListItem)
            {
                match += MatchValue(node1, ((DataVisitListItem)node1).Item, node2, ((DataVisitListItem)node2).Item);
            }

            match += MatchValues(node1, node1.Members, node2, node2.Members, true);
            match += MatchValues(node1, node1.Items, node2, node2.Items, false);
            return match;
        }

        private DataMatch MatchValue(IDataVisitNode node1, object value1, IDataVisitNode node2, object value2)
        {
            // Match null vs non null and types
            if (value1 == null || value2 == null || value1.GetType() != value2.GetType())
            {
                return ReferenceEquals(value1, value2) ? DataMatch.MatchOne : UnMatched(node1, node2);
            }

            // Match value (only for value types)
            // Matching of members is done in Match(DataVisitNode, DataVisitNode)
            var type = descriptorFactory.Find(value1.GetType());
            if (DataVisitNode.IsComparableOnlyType(value1, type))
            {
                return Equals(value1, value2) ? DataMatch.MatchOne : UnMatched(node1, node2);
            }

            // Empty match
            return DataMatch.Empty;
        }

        private DataMatch MatchValues(DataVisitNode node1Parent, List<IDataVisitNode> nodes1, DataVisitNode node2Parent, List<IDataVisitNode> nodes2, bool expectSameCount)
        {
            if (nodes1 == null || nodes2 == null || (expectSameCount && nodes1.Count != nodes2.Count))
            {
                return ReferenceEquals(nodes1, nodes2) ? DataMatch.Empty : UnMatched(node1Parent, node2Parent);
            }

            var match = new DataMatch();

            if (expectSameCount)
            {
                for (int i = 0; i < nodes1.Count; i++)
                {
                    match += Match((DataVisitNode)nodes1[i], (DataVisitNode)nodes2[i]);
                }
            }
            else
            {
                var alignedDiffs = Diff2.CompareAndAlign(nodes1, nodes2, modelNodeComparer, modelNodeSimilarityComparer, modelNodeCanAlign).ToList();
                foreach (var alignedDiffChange in alignedDiffs)
                {
                    switch (alignedDiffChange.Change)
                    {
                        case ChangeType.Same:
                        case ChangeType.Changed:
                            match += Match((DataVisitNode)nodes1[alignedDiffChange.Index1], (DataVisitNode)nodes2[alignedDiffChange.Index2]);
                            break;
                        case ChangeType.Added:
                            match += new DataMatch(0, nodes2[alignedDiffChange.Index2].CountNodes());
                            break;
                        case ChangeType.Deleted:
                            match += new DataMatch(0, nodes1[alignedDiffChange.Index1].CountNodes());
                            break;
                    }
                }
            }

            return match;
        }

        private static DataMatch UnMatched(IDataVisitNode node1, IDataVisitNode node2)
        {
            return new DataMatch(0, Math.Max(node1 != null ? node1.CountNodes() : 0, node2 != null ? node2.CountNodes() : 0));
        }

        private class ModelNodeComparer : IEqualityComparer<IDataVisitNode>
        {
            private readonly DataMatcher matcher;

            public ModelNodeComparer(DataMatcher matcher)
            {
                this.matcher = matcher;
            }

            public bool Equals(IDataVisitNode x, IDataVisitNode y)
            {
                // An equatable is a perfect match
                return matcher.Match((DataVisitNode)x, (DataVisitNode)y).Succeed;
            }

            public int GetHashCode(IDataVisitNode obj)
            {
                // We always return the same hashcode
                return 0;
            }
        }

        private class ModelNodeSimilarityComparer : ISimilarityComparer<IDataVisitNode>
        {
            private readonly DataMatcher matcher;

            public ModelNodeSimilarityComparer(DataMatcher matcher)
            {
                this.matcher = matcher;
            }

            public double Compare(IDataVisitNode value1, IDataVisitNode value2)
            {
                var result = matcher.Match((DataVisitNode)value1, (DataVisitNode)value2);
                return (double)result.Count/result.Total;
            }
        }

        private class ModelNodeCanAlign : IAlignmentFilter<IDataVisitNode>
        {
            private readonly ModelNodeSimilarityComparer comparer;

            public ModelNodeCanAlign(ModelNodeSimilarityComparer comparer)
            {
                this.comparer = comparer;
            }

            public bool CanAlign(IDataVisitNode value1, IDataVisitNode value2)
            {
                return comparer.Compare(value1, value2) > 0.1;
            }
        }

        private struct NodeKey : IEquatable<NodeKey>
        {
            private readonly DataVisitNode node1;
            private readonly DataVisitNode node2;

            public NodeKey(DataVisitNode node1, DataVisitNode node2)
            {
                this.node1 = node1;
                this.node2 = node2;
            }

            public bool Equals(NodeKey other)
            {
                return ReferenceEquals(node1, other.node1) && ReferenceEquals(node2, other.node2);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is NodeKey && Equals((NodeKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((node1 != null ? node1.GetHashCode() : 0) * 397) ^ (node2 != null ? node2.GetHashCode() : 0);
                }
            }

            public static bool operator ==(NodeKey left, NodeKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(NodeKey left, NodeKey right)
            {
                return !left.Equals(right);
            }
        }
    }
}