using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Analysis
{
    public static class AssetCompositeAnalysis
    {
        public static void FixupAssetPartReferences(AssetComposite assetComposite, Func<object, object> resolver)
        {
            var references = Visit(assetComposite);

            // Reverse the list, so that we can still properly update everything
            // (i.e. if we have a[0], a[1], a[1].Test, we have to do it from back to front to be valid at each step)
            references.Reverse();

            foreach (var reference in references)
            {
                var realPart = resolver(reference.AssetPart);
                reference.Path.Apply(assetComposite, MemberPathAction.ValueSet, realPart);
            }
        }

        private static List<AssetCompositeReferenceAnalysis.AssetPartReference> Visit(AssetComposite assetComposite)
        {
            if (assetComposite == null) throw new ArgumentNullException(nameof(assetComposite));

            var entityReferenceVistor = new AssetCompositeReferenceAnalysis();
            entityReferenceVistor.VisitAsset(assetComposite);
            return entityReferenceVistor.Result;
        }

        private class AssetCompositeReferenceAnalysis : AssetVisitorBase
        {
            public struct AssetPartReference
            {
                public object AssetPart;
                public MemberPath Path;
            }

            private AssetCompositeVisitorContext context;

            public List<AssetPartReference> Result { get; } = new List<AssetPartReference>();

            public void VisitAsset(AssetComposite asset)
            {
                Result.Clear();
                context = new AssetCompositeVisitorContext(asset.GetType());
                Visit(asset);
                context = null;
            }

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
                context.LeaveNode(descriptor.Type, shouldRemove);
            }
        }
    }
}
