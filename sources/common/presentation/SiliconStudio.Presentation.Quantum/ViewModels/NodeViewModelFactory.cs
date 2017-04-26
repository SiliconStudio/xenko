// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Presentation.Quantum.Presenters;

namespace SiliconStudio.Presentation.Quantum.ViewModels
{
    public class NodeViewModelFactory : INodeViewModelFactory
    {
        public NodeViewModel CreateGraph(GraphViewModel owner, Type rootType, IEnumerable<INodePresenter> rootNodes)
        {
            var rootViewModelNode = CreateNodeViewModel(owner, null, rootType, rootNodes.ToList(), true);
            return rootViewModelNode;
        }

        public void GenerateChildren(GraphViewModel owner, NodeViewModel parent, List<INodePresenter> nodePresenters)
        {
            foreach (var child in CombineChildren(nodePresenters))
            {
                if (ShouldConstructViewModel(child))
                {
                    Type type = null;
                    var typeMatch = true;
                    foreach (var childPresenter in child)
                    {
                        if (type == null)
                        {
                            type = childPresenter.Type;
                        }
                        else if (type != childPresenter.Type && type.IsAssignableFrom(childPresenter.Type))
                        {
                            type = childPresenter.Type;
                        }
                        else if (type != childPresenter.Type)
                        {
                            typeMatch = false;
                            break;
                        }
                    }
                    if (typeMatch)
                    {
                        CreateNodeViewModel(owner, parent, child.First().Type, child);
                    }
                }
            }
        }

        protected virtual NodeViewModel CreateNodeViewModel(GraphViewModel owner, NodeViewModel parent, Type nodeType, List<INodePresenter> nodePresenters, bool isRootNode = false)
        {
            // TODO: properly compute the name
            var viewModel = new NodeViewModel(owner, parent, nodePresenters.First().Name, nodeType, nodePresenters);
            if (isRootNode)
            {
                owner.RootNode = viewModel;
            }
            viewModel.Refresh();
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
