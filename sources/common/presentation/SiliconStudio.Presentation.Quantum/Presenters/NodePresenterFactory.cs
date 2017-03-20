using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class NodePresenterFactory : INodePresenterFactory, INodePresenterFactoryInternal
    {
        public NodePresenterFactory(IReadOnlyCollection<INodePresenterCommand> availableCommands)
        {
            AvailableCommands = availableCommands;
        }

        public IReadOnlyCollection<INodePresenterCommand> AvailableCommands { get; }

        [NotNull]
        public INodePresenter CreateNodeHierarchy(IObjectNode rootNode, GraphNodePath rootNodePath, IPropertiesProviderViewModel propertyProvider)
        {
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));
            var rootPresenter = CreateRootPresenter(rootNode);
            CreateChildren(rootPresenter, rootNode, propertyProvider);
            return rootPresenter;
        }

        public void CreateChildren(IInitializingNodePresenter parentPresenter, IObjectNode objectNode, IPropertiesProviderViewModel propertyProvider)
        {
            if (parentPresenter == null) throw new ArgumentNullException(nameof(parentPresenter));
            if (objectNode == null) throw new ArgumentNullException(nameof(objectNode));
            CreateMembers(parentPresenter, objectNode, propertyProvider);
            CreateItems(parentPresenter, objectNode, propertyProvider);
            parentPresenter.FinalizeInitialization();
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateRootPresenter([NotNull] IObjectNode rootNode)
        {
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));
            return new RootNodePresenter(this, rootNode);
        }

        protected virtual bool ShouldCreateMemberPresenter([NotNull] INodePresenter parent, [NotNull] IMemberNode member, [CanBeNull] IPropertiesProviderViewModel propertyProvider)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (member == null) throw new ArgumentNullException(nameof(member));
            // Ask the property provider if we have one, otherwise always construct.
            return propertyProvider?.ShouldConstructMember(member) ?? true;
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateMember([NotNull] INodePresenter parentPresenter, [NotNull] IMemberNode member)
        {
            if (parentPresenter == null) throw new ArgumentNullException(nameof(parentPresenter));
            if (member == null) throw new ArgumentNullException(nameof(member));
            return new MemberNodePresenter(this, parentPresenter, member);
        }

        protected virtual bool ShouldCreateItemPresenter([NotNull] INodePresenter parent, [NotNull] IObjectNode collectionNode, Index index, [CanBeNull] IPropertiesProviderViewModel propertyProvider)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (collectionNode == null) throw new ArgumentNullException(nameof(collectionNode));
            // Ask the property provider if we have one, otherwise always construct.
            return propertyProvider?.ShouldConstructItem(collectionNode, index) ?? true;
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateItem([NotNull] INodePresenter containerPresenter, [NotNull] IObjectNode containerNode, Index index)
        {
            if (containerPresenter == null) throw new ArgumentNullException(nameof(containerPresenter));
            if (containerNode == null) throw new ArgumentNullException(nameof(containerNode));
            return new ItemNodePresenter(this, containerPresenter, containerNode, index);
        }

        private void CreateMembers(IInitializingNodePresenter parentPresenter, IObjectNode objectNode, IPropertiesProviderViewModel propertyProvider)
        {
            foreach (var member in objectNode.Members)
            {
                if (ShouldCreateMemberPresenter(parentPresenter, member, propertyProvider))
                {
                    var memberPresenter = CreateMember(parentPresenter, member);
                    if (member.Target != null)
                    {
                        CreateChildren(memberPresenter, member.Target, propertyProvider);
                    }
                    parentPresenter.AddChild(memberPresenter);
                }
            }
        }

        private void CreateItems(IInitializingNodePresenter parentPresenter, IObjectNode objectNode, IPropertiesProviderViewModel propertyProvider)
        {
            if (objectNode.IsEnumerable)
            {
                if (objectNode.ItemReferences != null)
                {
                    foreach (var item in objectNode.ItemReferences)
                    {
                        if (ShouldCreateItemPresenter(parentPresenter, objectNode, item.Index, propertyProvider))
                        {
                            var itemPresenter = CreateItem(parentPresenter, objectNode, item.Index);
                            if (item.TargetNode != null)
                            {
                                CreateChildren(itemPresenter, item.TargetNode, propertyProvider);
                            }
                            parentPresenter.AddChild(itemPresenter);
                        }
                    }
                }
                else
                {
                    foreach (var item in objectNode.Indices)
                    {
                        if (ShouldCreateItemPresenter(parentPresenter, objectNode, item, propertyProvider))
                        {
                            var itemPresenter = CreateItem(parentPresenter, objectNode, item);
                            parentPresenter.AddChild(itemPresenter);
                        }
                    }
                }
            }
        }
    }
}
