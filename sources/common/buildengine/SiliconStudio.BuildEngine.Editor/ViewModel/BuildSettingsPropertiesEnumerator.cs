using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

using Microsoft.WindowsAPICodePack.Dialogs;

using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    class BuildSettingsPropertiesEnumerator : IChildrenPropertyEnumerator
    {
        public enum VisualizationTemplate
        {
            BrowseFile,
            ReadOnlyText,
            ReadOnlyBoolean,
            ButtonCommand,
        }

        public const string SourceBaseDirectoryPropertyName = "SourceBaseDirectory";
        public const string BuildDirectoryPropertyName = "BuildDirectory";
        public const string OutputDirectoryPropertyName = "OutputDirectory";
        public const string AbsoluteSourceBaseDirectoryPropertyName = "AbsoluteSourceBaseDirectory";
        public const string AbsoluteBuildDirectoryPropertyName = "AbsoluteBuildDirectory";
        public const string AbsoluteOutputDirectoryPropertyName = "AbsoluteOutputDirectory";
        public const string AbsoluteMetadataDatabaseDirectoryPropertyName = "AbsoluteMetadataDatabaseDirectory";
        public const string ScriptPathPropertyName = "ScriptPath";
        public const string ScriptFolderPropertyName = "ScriptFolder";
        public const string MetadataDatabaseDirectoryPropertyName = "MetadataDatabaseDirectory";
        public const string IsMetadataDatabaseOpenedPropertyName = "IsMetadataDatabaseOpened";
        public const string CreateDatabasePropertyName = "CreateDatabase";
        public const string AvailableKeysPropertyName = "AvailableKeys";

        private static void BrowseCommand(IViewModelNode viewModelNode, object parameter)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };

            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                viewModelNode.Parent.Content.Value = dialog.FileName;
            }
        }

        private static void ToggleRelativePathCommand(IViewModelNode viewModelNode, IViewModelNode modelParentNode, string pathRelativeRootProperty, string pathRelativeTargetProperty)
        {
            IViewModelNode relativePathRootNode = modelParentNode.GetChild(pathRelativeRootProperty);
            IViewModelNode relativePathTargetNode = modelParentNode.GetChild(pathRelativeTargetProperty);
            if (relativePathRootNode != null && relativePathTargetNode != null)
            {
                var currentPath = (string)viewModelNode.Parent.Content.Value;
                if (string.IsNullOrWhiteSpace(currentPath))
                {
                    MessageBox.Show("No directory set. You must first set a directory in order to toggle absolute/relative path.", "BuildEditor", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                if (!PathExt.IsValidPath(currentPath))
                {
                    MessageBox.Show("Directory is invalid. Please set a valid directory.", "BuildEditor", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                bool toRelativePath = currentPath.Contains(Path.VolumeSeparatorChar);
                var relativeTarget = (string)relativePathTargetNode.Content.Value;

                if (toRelativePath)
                {
                    var relativeRoot = (string)relativePathRootNode.Content.Value;

                    if (string.IsNullOrWhiteSpace(relativeRoot) || !PathExt.IsValidPath(relativeRoot))
                    {
                        MessageBox.Show("Please save your script to toggle this directory to a relative path.", "BuildEditor", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    if (string.IsNullOrEmpty(relativeTarget) || !PathExt.IsValidPath(relativeTarget))
                        viewModelNode.Parent.Content.Value = ".";
                    else
                        viewModelNode.Parent.Content.Value = PathExt.GetRelativePath(relativeRoot, relativeTarget);
                }
                else
                {
                    viewModelNode.Parent.Content.Value = relativeTarget;
                }
            }
        }

        private static void AddNewMetadataKey(IViewModelNode viewModelNode)
        {
            IViewModelNode newKeyNameNode = viewModelNode.Parent.GetChild("NewKeyName");
            IViewModelNode newKeyTypeNode = viewModelNode.Parent.GetChild("NewKeyType");

            var newKey = new MetadataKey((string)newKeyNameNode.Content.Value, (MetadataKey.DatabaseType)newKeyTypeNode.Content.Value);

            if (!newKey.IsValid())
            {
                MessageBox.Show("Invalid key name. A key name must contain only alphanumeric characters.", "BuildEditor", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!BuildEditionViewModel.GetActiveSession().CreateMetadataKey(newKey))
                MessageBox.Show("Unable to add this key. Verify that a key with the same name doesn't already exist.", "BuildEditor", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void RemoveMetadataKey(IViewModelNode viewModelNode, object parameter)
        {
            var key = (MetadataKey)parameter;

            if (!BuildEditionViewModel.GetActiveSession().DeleteMetadataKey(key))
                MessageBox.Show("Unable to remove this key. Verify that a key is not used anymore.", "BuildEditor", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void AddBrowsablePropertyNode(IViewModelNode parentNode, string propertyName, string pathRelativeRootProperty, string pathRelativeTargetProperty, IViewModelNode modelParentNode = null)
        {
            var propertyNode = ViewModelConstructor.AddPropertyNode(parentNode, propertyName);

            ViewModelConstructor.AddCommandNode(propertyNode, "BrowseCommand", BrowseCommand, ViewModelContentFlags.HiddenContent);
            ViewModelConstructor.AddCommandNode(propertyNode, "ToggleRelativePathCommand", (viewModel, parameter) => ToggleRelativePathCommand(viewModel, modelParentNode ?? parentNode, pathRelativeRootProperty, pathRelativeTargetProperty), ViewModelContentFlags.HiddenContent);
        }

        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            var session = (BuildSessionViewModel)viewModelNode.Content.Value;

            // Global properties
            ViewModelConstructor.AddPropertyNode(viewModelNode, ScriptPathPropertyName);
            AddBrowsablePropertyNode(viewModelNode, SourceBaseDirectoryPropertyName, ScriptFolderPropertyName, AbsoluteSourceBaseDirectoryPropertyName);
            AddBrowsablePropertyNode(viewModelNode, BuildDirectoryPropertyName, ScriptFolderPropertyName, AbsoluteBuildDirectoryPropertyName);
            AddBrowsablePropertyNode(viewModelNode, OutputDirectoryPropertyName, ScriptFolderPropertyName, AbsoluteOutputDirectoryPropertyName);

            // Metadata properties
            var metadataCategoryNode = ViewModelConstructor.AddCategoryNode(viewModelNode, "Metadata properties");
            AddBrowsablePropertyNode(metadataCategoryNode, MetadataDatabaseDirectoryPropertyName, ScriptFolderPropertyName, AbsoluteMetadataDatabaseDirectoryPropertyName, viewModelNode);
            ViewModelConstructor.AddPropertyNode(metadataCategoryNode, IsMetadataDatabaseOpenedPropertyName);
            ViewModelConstructor.AddCommandNode(metadataCategoryNode, "CreateDatabaseCommand", (v, p) => BuildEditionViewModel.GetActiveSession().CreateMetadataDatabase());
            var availableKeys = ViewModelConstructor.AddContentListNode(metadataCategoryNode, AvailableKeysPropertyName, session.AvailableKeys);
            ViewModelConstructor.AddCommandNode(availableKeys, "Add", ((viewModel, parameter) => AddNewMetadataKey(viewModel)));
            ViewModelConstructor.AddCommandNode(availableKeys, "Remove", (RemoveMetadataKey));
            ViewModelConstructor.AddValueNode(availableKeys, "NewKeyName", "");
            ViewModelConstructor.AddValueNode(availableKeys, "NewKeyType", MetadataKey.DatabaseType.String);

            // Hidden properties
            ViewModelConstructor.AddPropertyNode(viewModelNode, ScriptFolderPropertyName, ViewModelContentFlags.HiddenContent);
            ViewModelConstructor.AddPropertyNode(viewModelNode, AbsoluteSourceBaseDirectoryPropertyName, ViewModelContentFlags.HiddenContent);
            ViewModelConstructor.AddPropertyNode(viewModelNode, AbsoluteBuildDirectoryPropertyName, ViewModelContentFlags.HiddenContent);
            ViewModelConstructor.AddPropertyNode(viewModelNode, AbsoluteOutputDirectoryPropertyName, ViewModelContentFlags.HiddenContent);
            ViewModelConstructor.AddPropertyNode(viewModelNode, AbsoluteMetadataDatabaseDirectoryPropertyName, ViewModelContentFlags.HiddenContent);
        }
    }
}
