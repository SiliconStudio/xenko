using System;
using System.Linq;
using System.Reflection;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Engine.Data;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Assets.Model
{
    /// <summary>
    /// Transforms <see cref="EntityHierarchyData"/> nodes into hierarchical <see cref="EntityDiffNode"/>.
    /// </summary>
    [DiffNodeBuilder]
    public class EntityHierarchyDiffNodeBuilder : IDataCustomVisitor
    {
        public bool CanVisit(Type type)
        {
            return (type == typeof(EntityHierarchyData) || type == typeof(TransformationComponentData));
        }

        public void Visit(ref VisitorContext context)
        {
            var dataVisitNodeBuilder = (DataVisitNodeBuilder)context.Visitor;

            if (context.Instance is EntityHierarchyData)
            {
                // Create alternative "proxy" object to run diff on
                var entityHierarchy = (EntityHierarchyData)context.Instance;
                var entityCollectionProxy = new EntityDiffNode(entityHierarchy, entityHierarchy.RootEntity);

                // Add this object as member, so that it gets processed instead
                dataVisitNodeBuilder.VisitObjectMember(context.Instance, context.Descriptor, new ConvertedDescriptor(context.DescriptorFactory, "Entities", entityCollectionProxy), entityCollectionProxy);
            }
            else if (context.Instance is TransformationComponentData)
            {
                // Visit object, as usual
                context.Visitor.VisitObject(context.Instance, context.Descriptor, true);

                // Remove TransformationComponentData.Children
                // We don't want any conflict here, as it will be computed back by EntityDiffNode.Children
                var currentNode = dataVisitNodeBuilder.CurrentNode;
                currentNode.Members.RemoveWhere(x => ((DataVisitMember)x).MemberDescriptor.Name == "Children");
            }
        }
    }
}