using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class NodePresenterFactory : INodePresenterFactoryInternal
    {
        [NotNull] private readonly INodeBuilder nodeBuilder;

        public NodePresenterFactory([NotNull] INodeBuilder nodeBuilder, [NotNull] IReadOnlyCollection<INodePresenterCommand> availableCommands, [NotNull] IReadOnlyCollection<INodePresenterUpdater> availableUpdaters)
        {
            this.nodeBuilder = nodeBuilder;
            if (nodeBuilder == null) throw new ArgumentNullException(nameof(nodeBuilder));
            if (availableCommands == null) throw new ArgumentNullException(nameof(availableCommands));
            if (availableUpdaters == null) throw new ArgumentNullException(nameof(availableUpdaters));
            AvailableCommands = availableCommands;
            AvailableUpdaters = availableUpdaters;
        }

        public IReadOnlyCollection<INodePresenterCommand> AvailableCommands { get; }

        public IReadOnlyCollection<INodePresenterUpdater> AvailableUpdaters { get; }

        public bool IsPrimitiveType(Type type)
        {
            return nodeBuilder.IsPrimitiveType(type);
        }

        [NotNull]
        public INodePresenter CreateNodeHierarchy(IObjectNode rootNode, GraphNodePath rootNodePath, IPropertyProviderViewModel propertyProvider)
        {
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));
            var rootPresenter = CreateRootPresenter(propertyProvider, rootNode);
            GenerateChildren(rootPresenter, rootNode, propertyProvider);
            RunUpdaters(rootPresenter);
            return rootPresenter;
        }

        public void CreateChildren(IInitializingNodePresenter parentPresenter, IObjectNode objectNode, IPropertyProviderViewModel propertyProvider)
        {
            GenerateChildren(parentPresenter, objectNode, propertyProvider);
            RunUpdaters(parentPresenter);
        }

        private void GenerateChildren(IInitializingNodePresenter parentPresenter, IObjectNode objectNode, IPropertyProviderViewModel propertyProvider)
        {
            if (parentPresenter == null) throw new ArgumentNullException(nameof(parentPresenter));
            if (objectNode == null) throw new ArgumentNullException(nameof(objectNode));
            CreateMembers(propertyProvider, parentPresenter, objectNode);
            CreateItems(propertyProvider, parentPresenter, objectNode);
            parentPresenter.FinalizeInitialization();
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateRootPresenter(IPropertyProviderViewModel propertyProvider, [NotNull] IObjectNode rootNode)
        {
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));
            return new RootNodePresenter(this, propertyProvider, rootNode);
        }

        protected virtual bool ShouldCreateMemberPresenter([NotNull] INodePresenter parent, [NotNull] IMemberNode member, [CanBeNull] IPropertyProviderViewModel propertyProvider)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (member == null) throw new ArgumentNullException(nameof(member));
            // Ask the property provider if we have one, otherwise always construct.
            return propertyProvider?.ShouldConstructMember(member) ?? true;
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateMember(IPropertyProviderViewModel propertyProvider, [NotNull] INodePresenter parentPresenter, [NotNull] IMemberNode member)
        {
            if (parentPresenter == null) throw new ArgumentNullException(nameof(parentPresenter));
            if (member == null) throw new ArgumentNullException(nameof(member));
            return new MemberNodePresenter(this, propertyProvider, parentPresenter, member);
        }

        protected virtual bool ShouldCreateItemPresenter([NotNull] INodePresenter parent, [NotNull] IObjectNode collectionNode, Index index, [CanBeNull] IPropertyProviderViewModel propertyProvider)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (collectionNode == null) throw new ArgumentNullException(nameof(collectionNode));
            // Ask the property provider if we have one, otherwise always construct.
            return propertyProvider?.ShouldConstructItem(collectionNode, index) ?? true;
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateItem(IPropertyProviderViewModel propertyProvider, [NotNull] INodePresenter containerPresenter, [NotNull] IObjectNode containerNode, Index index)
        {
            if (containerPresenter == null) throw new ArgumentNullException(nameof(containerPresenter));
            if (containerNode == null) throw new ArgumentNullException(nameof(containerNode));
            return new ItemNodePresenter(this, propertyProvider, containerPresenter, containerNode, index);
        }

        private void CreateMembers(IPropertyProviderViewModel propertyProvider, IInitializingNodePresenter parentPresenter, IObjectNode objectNode)
        {
            foreach (var member in objectNode.Members)
            {
                if (ShouldCreateMemberPresenter(parentPresenter, member, propertyProvider))
                {
                    var memberPresenter = CreateMember(propertyProvider, parentPresenter, member);
                    if (member.Target != null)
                    {
                        GenerateChildren(memberPresenter, member.Target, propertyProvider);
                    }
                    parentPresenter.AddChild(memberPresenter);
                    RunUpdaters(memberPresenter);
                }
            }
        }

        private void CreateItems(IPropertyProviderViewModel propertyProvider, IInitializingNodePresenter parentPresenter, IObjectNode objectNode)
        {
            if (objectNode.IsEnumerable)
            {
                if (objectNode.ItemReferences != null)
                {
                    foreach (var item in objectNode.ItemReferences)
                    {
                        if (ShouldCreateItemPresenter(parentPresenter, objectNode, item.Index, propertyProvider))
                        {
                            var itemPresenter = CreateItem(propertyProvider, parentPresenter, objectNode, item.Index);
                            if (item.TargetNode != null)
                            {
                                GenerateChildren(itemPresenter, item.TargetNode, propertyProvider);
                            }
                            parentPresenter.AddChild(itemPresenter);
                            RunUpdaters(itemPresenter);
                        }
                    }
                }
                else
                {
                    foreach (var item in objectNode.Indices)
                    {
                        if (ShouldCreateItemPresenter(parentPresenter, objectNode, item, propertyProvider))
                        {
                            var itemPresenter = CreateItem(propertyProvider, parentPresenter, objectNode, item);
                            parentPresenter.AddChild(itemPresenter);
                            RunUpdaters(itemPresenter);
                        }
                    }
                }
            }
        }

        protected void RunUpdaters(IInitializingNodePresenter nodePresenter)
        {
            foreach (var updater in AvailableUpdaters)
            {
                updater.UpdateNode(nodePresenter);
            }
        }
    }
}
