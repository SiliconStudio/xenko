using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using SiliconStudio.Presentation.Behaviors;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    public class DropCommandEnumerator : IChildrenPropertyEnumerator
    {
        // TODO/Benlitz: fix asyncness of this command (need to invoke a dispatcher from the ObservableViewModelNode which is not implemented yet)
        //private readonly Func<IViewModelNode, DropCommandParameters, Task> dropAction;
        private readonly Action<IViewModelNode, DropCommandParameters> dropAction;

        public DropCommandEnumerator(Action<IViewModelNode, DropCommandParameters> dropAction)
        {
            this.dropAction = dropAction;
        }

        /// <inheritdoc/>
        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            var processedNode = new List<IViewModelNode>();
            AddDropCommand(viewModelNode, processedNode);
        }

        private void AddDropCommand(IViewModelNode viewModelNode, ICollection<IViewModelNode> processedNode, bool recursive = true)
        {
            if (processedNode.Contains(viewModelNode))
                return;

            if (viewModelNode.Children.All(x => x.Name != "DropCommand"))
            {
                Type contentType = viewModelNode.Content.GetType();

                if (contentType == typeof(PropertyInfoViewModelContent))
                {
                    var node = new ViewModelNode("DropCommand", new RootViewModelContent((ExecuteCommand)(
                        (viewModel, parameter) => dropAction(viewModel, parameter as DropCommandParameters)))) { Content = { Flags = ViewModelContentFlags.HiddenContent } };
                    viewModelNode.Children.Add(node);
                }
                else if (contentType.IsGenericType && contentType.GetGenericTypeDefinition() == typeof(EnumerableViewModelContent<>))
                {
                    var node = new ViewModelNode("DropCommand", new RootViewModelContent((ExecuteCommand)(
                       (viewModel, parameter) =>
                       {
                           var drop = (DropCommandParameters)parameter;
                           var content = (PropertyInfoViewModelContent)(((FrameworkElement)drop.Sender).DataContext);
                           drop.TargetIndex = (int)content.Index[0];
                           dropAction(viewModel, drop);
                       }))) { Content = { Flags = ViewModelContentFlags.HiddenContent } };

                    viewModelNode.Children.Add(node);
                }
            }
            processedNode.Add(viewModelNode);

            if (recursive)
            {
                foreach (var childNode in viewModelNode.Children)
                {
                    AddDropCommand(childNode, processedNode);
                }
            }

        }
    }
}