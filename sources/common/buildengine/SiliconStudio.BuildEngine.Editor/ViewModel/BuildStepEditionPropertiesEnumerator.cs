using System.Linq;
using System.Reflection;

using SiliconStudio.BuildEngine.Editor.Model;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    public class BuildStepEditionPropertiesEnumerator : IChildrenPropertyEnumerator
    {
        /// <inheritdoc/>
        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            var buildStep = viewModelNode.Content.Value as BuildStep;
            if (buildStep == null)
                return;

            // Hide execution properties
            viewModelNode.Children.Single(x => x.Name == BuildStepPropertiesEnumerator.ExecutionIdPropertyName).Content.Flags |= ViewModelContentFlags.HiddenContent;
            viewModelNode.Children.Single(x => x.Name == BuildStepPropertiesEnumerator.StatusPropertyName).Content.Flags |= ViewModelContentFlags.HiddenContent;

            var fileEnumBuildStep = buildStep as FileEnumerationBuildStep;
            if (fileEnumBuildStep != null)
            {
                // Override default Steps property
                ViewModelConstructor.AddReferenceEnumerableNode(viewModelNode, BuildStepPropertiesEnumerator.StepsPropertyName, fileEnumBuildStep.GetChildSteps(), context, ViewModelContentFlags.HiddenContent);

                // Add Add/Remove commands to the SearchPattern property
                IViewModelNode searchPattern = viewModelNode.Children.Single(x => x.Name == BuildStepPropertiesEnumerator.SearchPatternPropertyName);
                PropertyInfo searchPropertyInfo = fileEnumBuildStep.GetType().GetProperty(BuildStepPropertiesEnumerator.SearchPatternPropertyName);
                ViewModelConstructor.AddCommandNode(searchPattern, "Add", ((viewModel, parameter) => BuildEditionViewModel.GetActiveSession().AddNewItemToListProperty(viewModel, searchPropertyInfo)));
                ViewModelConstructor.AddCommandNode(searchPattern, "Remove", ((viewModel, parameter) => BuildEditionViewModel.GetActiveSession().RemoveItemToListProperty(viewModel, searchPropertyInfo, (PropertyInfoViewModelContent)parameter)));

                // Add Add/Remove commands to the ExcludePattern property
                IViewModelNode excludePattern = viewModelNode.Children.Single(x => x.Name == BuildStepPropertiesEnumerator.ExcludePatternPropertyName);
                PropertyInfo excludePropertyInfo = fileEnumBuildStep.GetType().GetProperty(BuildStepPropertiesEnumerator.ExcludePatternPropertyName);
                ViewModelConstructor.AddCommandNode(excludePattern, "Add", ((viewModel, parameter) => BuildEditionViewModel.GetActiveSession().AddNewItemToListProperty(viewModel, excludePropertyInfo)));
                ViewModelConstructor.AddCommandNode(excludePattern, "Remove", ((viewModel, parameter) => BuildEditionViewModel.GetActiveSession().RemoveItemToListProperty(viewModel, excludePropertyInfo, (PropertyInfoViewModelContent)parameter)));
            }

            var outputEnumBuildStep = buildStep as OutputEnumerationBuildStep;
            if (outputEnumBuildStep != null)
            {
                // Override default Steps property
                ViewModelConstructor.AddReferenceEnumerableNode(viewModelNode, BuildStepPropertiesEnumerator.StepsPropertyName, outputEnumBuildStep.GetChildSteps(), context, ViewModelContentFlags.HiddenContent);

                // Add Add/Remove commands to the SearchTags property
                IViewModelNode searchTags = viewModelNode.Children.Single(x => x.Name == BuildStepPropertiesEnumerator.SearchTagsPropertyName);
                PropertyInfo searchTagsPropertyInfo = outputEnumBuildStep.GetType().GetProperty(BuildStepPropertiesEnumerator.SearchTagsPropertyName);
                ViewModelConstructor.AddCommandNode(searchTags, "Add", ((viewModel, parameter) => BuildEditionViewModel.GetActiveSession().AddNewItemToListProperty(viewModel, searchTagsPropertyInfo)));
                ViewModelConstructor.AddCommandNode(searchTags, "Remove", ((viewModel, parameter) => BuildEditionViewModel.GetActiveSession().RemoveItemToListProperty(viewModel, searchTagsPropertyInfo, (PropertyInfoViewModelContent)parameter)));
            }

            if (buildStep.Parent != null)
            {
                ViewModelConstructor.AddCommandNode(viewModelNode, "Delete", (vm, p) => BuildEditionViewModel.GetActiveSession().DeleteStep(buildStep), ViewModelContentFlags.HiddenContent);
            }

            ViewModelConstructor.AddValueNode(viewModelNode, "IsRunning", false, ViewModelContentFlags.HiddenContent);
            ViewModelConstructor.AddValueNode(viewModelNode, "ExecutionStatus", ResultStatus.NotProcessed, ViewModelContentFlags.HiddenContent);

            ViewModelConstructor.AddValueNode(viewModelNode, "IsParallelToSelectedStep", false, ViewModelContentFlags.HiddenContent);
            ViewModelConstructor.AddValueNode(viewModelNode, "IsPrerequisiteToSelectedStep", false, ViewModelContentFlags.HiddenContent);
        }
    }
}
