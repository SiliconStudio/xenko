using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.DragNDrop;
using SiliconStudio.BuildEngine.Editor.Model;
using SiliconStudio.Presentation.Quantum.Legacy;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    public static class DraggableTreeViewModel
    {
        public static Func<bool> GetCanDrag(ObservableViewModelNode node)
        {
            return () => CanDrag(node);
        }

        public static Func<object> GetDrag(ObservableViewModelNode node)
        {
            return () => node;
        }

        public static Func<int, object, bool> GetCanInsert(ObservableViewModelNode node)
        {
            return (index, parent) => CanInsert(parent, node);
        }

        public static Action<int, object> GetInsert(ObservableViewModelNode node)
        {
            return (index, parent) => Task.Run(() => Insert(index, parent, node));
        }

        public static Func<string, bool> GetCanDropFormat(ObservableViewModelNode node)
        {
            return CanDropFormat;
        }

        public static Func<int, string, bool> GetCanInsertFormat(ObservableViewModelNode node)
        {
            return (i, s) => CanDropFormat(s);
        }

        public static Func<object, bool> GetCanDrop(ObservableViewModelNode node)
        {
            return obj => CanDrop(obj, node);
        }

        public static Action<object> GetDropAction(ObservableViewModelNode node)
        {
            return obj => Task.Run(() => DropAction(obj, node));
        }

        private static bool CanDrag(ObservableViewModelNode node)
        {
            if (BuildEditionViewModel.IsBuilding())
                return false;

            // Can't drag the root node
            var parentStep = node.GetChild("Parent") as ObservableViewModelNode;
            if (parentStep == null)
                return false;

            if (node.ModelNode.Content.Value is EmptyBuildStep)
                return false;

            var modelNode = parentStep.ModelNode;
            if (modelNode == null)
                return false;

            var value = modelNode.Content.Value as ViewModelReference;
            if (value == null)
                return false;

            return value.Model != null;
        }

        private static bool CanDropFormat(string s)
        {
            return true;
        }

        private static bool CanDrop(object obj, ObservableViewModelNode node)
        {
            var dragContent = obj as DragContent;
            if (dragContent == null)
                return false;

            // Can drop on an EmptyBuildStep
            if (node.ModelNode.Content.Value is EmptyBuildStep && dragContent.Items.Count() == 1)
                return true;

            // Can drop on a ListBuildStep
            var targetStep = (BuildStep)node.ModelNode.Content.Value;

            var firstItem = dragContent.Items.First();
            if (firstItem is Type)
            {
                return targetStep != null && targetStep.CanAddChildren(Enumerable.Empty<BuildStep>());
            }

            return targetStep != null && targetStep.CanAddChildren(dragContent.Items.Select(x => (BuildStep)((ObservableViewModelNode)x).ModelNode.Content.Value));
        }

        private static bool CanInsert(object obj, ObservableViewModelNode node)
        {
            var dragContent = obj as DragContent;
            if (dragContent == null)
                return false;

            // Can drop on an EmptyBuildStep
            if (node.ModelNode.Content.Value is EmptyBuildStep && dragContent.Items.Count() == 1)
                return true;

            // Can drop on a ListBuildStep
            var targetStep = (BuildStep)node.ModelNode.Content.Value;

            var firstItem = dragContent.Items.First();
            if (firstItem is Type)
            {
                return targetStep != null && targetStep.CanInsertChildren(Enumerable.Empty<BuildStep>());
            }

            return targetStep != null && targetStep.CanInsertChildren(dragContent.Items.Select(x => (BuildStep)((ObservableViewModelNode)x).ModelNode.Content.Value));
        }

        private static void DropAction(object obj, ObservableViewModelNode node)
        {
            var dragContent = obj as DragContent;
            if (dragContent == null)
                return;

            BuildStep[] steps;
            var firstItem = dragContent.Items.First();
            if (firstItem is Type)
                steps = (dragContent.Items.Select(x => BuildSessionViewModel.CreateStep((Type)x))).ToArray();
            else
                steps = (dragContent.Items.Select(x => (BuildStep)((ObservableViewModelNode)x).ModelNode.Content.Value)).ToArray();

            var parent = node.ModelNode.Content.Value as BuildStep;
            var emptyStep = parent as EmptyBuildStep;
            if (emptyStep != null)
            {
                emptyStep.ReplaceWith(steps.First());
            }
            else if (parent != null)
            {
                parent.AddChildren(steps);
            }
            BuildEditionViewModel.GetActiveSession().Refresh();
            // Select created steps
            if (firstItem is Type)
                BuildEditionViewModel.GetActiveSession().SelectBuildSteps(steps);
        }

        public static void Insert(int index, object obj, ObservableViewModelNode node)
        {
            var dragContent = obj as DragContent;
            if (dragContent == null)
                return;

            var parent = (ListBuildStep)node.ModelNode.Content.Value;
            var firstItem = dragContent.Items.First();
            BuildStep[] createdSteps = null;
            if (firstItem is Type)
            {
                createdSteps = dragContent.Items.Select(x => BuildSessionViewModel.CreateStep((Type)x)).ToArray();
                parent.InsertChildren(createdSteps, index);
            }
            else
                parent.InsertChildren(dragContent.Items.Select(x => (BuildStep)((ObservableViewModelNode)x).ModelNode.Content.Value), index);

            BuildEditionViewModel.GetActiveSession().Refresh();
            // Select created steps
            if (createdSteps != null)
                BuildEditionViewModel.GetActiveSession().SelectBuildSteps(createdSteps);
        }
    }
}
