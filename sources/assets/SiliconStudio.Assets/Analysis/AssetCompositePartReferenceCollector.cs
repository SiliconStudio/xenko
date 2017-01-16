using System.Collections.Generic;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// An asset visitor that collects all references to parts from a serialization point of view.
    /// </summary>
    public class AssetCompositePartReferenceCollector : AssetVisitorBase
    {
        private AssetCompositeVisitorContext context;

        /// <summary>
        /// A structure representing a reference to an asset part.
        /// </summary>
        public struct AssetPartReference
        {
            /// <summary>
            /// The referenced part.
            /// </summary>
            public object AssetPart;
            /// <summary>
            /// The path to the reference.
            /// </summary>
            public MemberPath Path;
        }

        /// <summary>
        /// Gets the list of references collected by this visitor after calling <see cref="VisitAsset"/>.
        /// </summary>
        public List<AssetPartReference> Result { get; } = new List<AssetPartReference>();

        /// <summary>
        /// Visits the given asset to collect references to asset part.
        /// </summary>
        /// <param name="asset">The asset to visit.</param>
        public void VisitAsset(AssetComposite asset)
        {
            Result.Clear();
            context = new AssetCompositeVisitorContext(asset.GetType());
            Visit(asset);
            context = null;
        }

        public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
        {
            var removeLastEnteredNode = context.EnterNode(member);
            try
            {
                base.VisitObjectMember(container, containerDescriptor, member, value);
            }
            finally
            {
                context.LeaveNode(removeLastEnteredNode);
            }
        }

        /// <inheritdoc/>
        public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
        {
            var shouldRemove = context.EnterNode(descriptor.Type);
            if (context.SerializeAsReference)
            {
                Result.Add(new AssetPartReference { AssetPart = obj, Path = CurrentPath.Clone() });
            }
            else
            {
                base.VisitObject(obj, descriptor, visitMembers);
            }
            context.LeaveNode(shouldRemove);
        }
    }
}
