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
        public INodePresenter CreateNodeHierarchy(IObjectNode rootNode, GraphNodePath rootNodePath)
        {
            var rootPresenter = CreateRootPresenter(rootNode);
            CreateChildren(rootPresenter, rootNode);
            return rootPresenter;
        }

        public void CreateChildren([NotNull] IInitializingNodePresenter parentPresenter, IObjectNode objectNode)
        {
            CreateMembers(parentPresenter, objectNode);
            CreateItems(parentPresenter, objectNode);
            parentPresenter.FinalizeInitialization();
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateRootPresenter([NotNull] IObjectNode rootNode)
        {
            if (rootNode == null) throw new ArgumentNullException(nameof(rootNode));
            return new RootNodePresenter(this, rootNode);
        }

        protected virtual bool ShouldCreateMemberPresenter([NotNull] IMemberNode member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            return true;
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateMember([NotNull] INodePresenter parentPresenter, [NotNull] IMemberNode member)
        {
            if (parentPresenter == null) throw new ArgumentNullException(nameof(parentPresenter));
            if (member == null) throw new ArgumentNullException(nameof(member));
            return new MemberNodePresenter(this, parentPresenter, member);
        }

        protected virtual bool ShouldCreateItemPresenter([NotNull] IObjectNode objectNode, Index item)
        {
            if (objectNode == null) throw new ArgumentNullException(nameof(objectNode));
            return true;
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateItem([NotNull] INodePresenter containerPresenter, [NotNull] IObjectNode containerNode, Index index)
        {
            if (containerPresenter == null) throw new ArgumentNullException(nameof(containerPresenter));
            if (containerNode == null) throw new ArgumentNullException(nameof(containerNode));
            return new ItemNodePresenter(this, containerPresenter, containerNode, index);
        }

        private void CreateMembers(IInitializingNodePresenter parentPresenter, IObjectNode objectNode)
        {
            foreach (var member in objectNode.Members)
            {
                if (ShouldCreateMemberPresenter(member))
                {
                    var memberPresenter = CreateMember(parentPresenter, member);
                    if (member.Target != null)
                    {
                        CreateChildren(memberPresenter, member.Target);
                    }
                    parentPresenter.AddChild(memberPresenter);
                }
            }
        }

        private void CreateItems(IInitializingNodePresenter parentPresenter, IObjectNode objectNode)
        {
            if (objectNode.IsEnumerable)
            {
                if (objectNode.ItemReferences != null)
                {
                    foreach (var item in objectNode.ItemReferences)
                    {
                        if (ShouldCreateItemPresenter(objectNode, item.Index))
                        {
                            var itemPresenter = CreateItem(parentPresenter, objectNode, item.Index);
                            if (item.TargetNode != null)
                            {
                                CreateChildren(itemPresenter, item.TargetNode);
                            }
                            parentPresenter.AddChild(itemPresenter);
                        }
                    }
                }
                else
                {
                    foreach (var item in objectNode.Indices)
                    {
                        if (ShouldCreateItemPresenter(objectNode, item))
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
