using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Presentation.Quantum.Presenters;

namespace SiliconStudio.Presentation.Quantum
{
    public class NodeViewModelFactory : INodeViewModelFactory
    {
        public NodeViewModel CreateGraph(GraphViewModel owner, Type rootType, IEnumerable<INodePresenter> rootNodes)
        {
            var rootViewModelNode = CreateNodeViewModel(owner, null, rootType, rootNodes.ToList(), true);
            return rootViewModelNode;
        }

        protected NodeViewModel CreateNodeViewModel(GraphViewModel owner, NodeViewModel parent, Type nodeType, List<INodePresenter> nodePresenters, bool isRootNode = false)
        {
            // TODO: properly compute the name
            var viewModel = new NodeViewModel(owner, parent, nodePresenters.First().Name, nodeType, nodePresenters);
            if (isRootNode)
            {
                owner.RootNode = viewModel;
            }
            GenerateChildren(owner, viewModel, nodePresenters);

            foreach (var nodePresenter in nodePresenters)
            {
                foreach (var command in nodePresenter.Commands)
                {
                    // TODO: review algorithm and properly implement CombineMode
                    if (viewModel.Commands.Cast<NodePresenterCommandWrapper>().All(x => x.Command != command))
                    {
                        var commandWrapper = new NodePresenterCommandWrapper(viewModel.ServiceProvider, nodePresenter, command);
                        viewModel.AddCommand(commandWrapper);
                    }
                }
                foreach (var attachedProperty in nodePresenter.AttachedProperties)
                {
                    viewModel.AddAssociatedData(attachedProperty.Key.Name, attachedProperty.Value);
                }
            }

            owner.GraphViewModelService?.NotifyNodeInitialized(viewModel);
            return viewModel;
        }

        protected virtual IEnumerable<List<INodePresenter>> CombineChildren(List<INodePresenter> nodePresenters)
        {
            var dictionary = new Dictionary<string, List<INodePresenter>>();
            foreach (var nodePresenter in nodePresenters)
            {
                foreach (var child in nodePresenter.Children)
                {
                    List<INodePresenter> presenters;
                    // TODO: properly implement CombineKey
                    if (!dictionary.TryGetValue(child.CombineKey, out presenters))
                    {
                        presenters = new List<INodePresenter>();
                        dictionary.Add(child.CombineKey, presenters);
                    }
                    presenters.Add(child);
                }
            }
            return dictionary.Values.Where(x => x.Count == nodePresenters.Count);
        }

        private void GenerateChildren(GraphViewModel owner, NodeViewModel parent, List<INodePresenter> nodePresenters)
        {
            foreach (var child in CombineChildren(nodePresenters))
            {
                if (ShouldConstructViewModel(child))
                {
                    // TODO: properly compute the type
                    CreateNodeViewModel(owner, parent, child.First().Type, child);
                }
            }
        }

        private static bool ShouldConstructViewModel(List<INodePresenter> nodePresenters)
        {
            foreach (var nodePresenter in nodePresenters)
            {
                var member = nodePresenter as MemberNodePresenter;
                var displayAttribute = member?.MemberAttributes.OfType<DisplayAttribute>().FirstOrDefault();
                if (displayAttribute != null && !displayAttribute.Browsable)
                    return false;
            }
            return true;
        }
    }
}