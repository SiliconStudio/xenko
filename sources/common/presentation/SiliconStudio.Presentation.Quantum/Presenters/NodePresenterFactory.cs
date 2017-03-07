using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public class NodePresenterFactory : INodePresenterFactory, INodePresenterFactoryInternal
    {
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

        [NotNull]
        protected virtual IInitializingNodePresenter CreateRootPresenter(IObjectNode rootNode)
        {
            return new RootNodePresenter(this, rootNode);
        }

        protected virtual bool ShouldCreateMemberPresenter(IMemberNode member)
        {
            return true;
        }

        protected virtual IInitializingNodePresenter CreateMember(INodePresenter parentPresenter, IMemberNode member)
        {
            return new MemberNodePresenter(this, parentPresenter, member);
        }

        protected virtual bool ShouldCreateItemPresenter(IObjectNode objectNode, Index item)
        {
            return true;
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateItem(INodePresenter containerPresenter, IObjectNode containerNode, Index index)
        {
            return new ItemNodePresenter(this, containerPresenter, containerNode, index);
        }
    }
}
