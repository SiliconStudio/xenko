using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using SiliconStudio.BuildEngine.Editor.ViewModel;
using SiliconStudio.Presentation.Quantum.Legacy;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.BuildEngine.Editor.View
{
    public class SettingsPropertyGridTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PathStringTemplate { get; set; }
        public DataTemplate ReadOnlyStringTemplate { get; set; }
        public DataTemplate ExpanderTemplate { get; set; }
        public DataTemplate ReadOnlyBoolTemplate { get; set; }
        public DataTemplate ButtonTemplate { get; set; }
        public DataTemplate MetadataKeyTemplate { get; set; }

        private static readonly DataTemplate EmptyTemplate = new DataTemplate();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (element == null)
                throw new Exception("Container must be of type FrameworkElement");

            var node = item as ObservableViewModelNode;
            if (node != null)
            {
                if (node.Content != null && (node.Content.Flags & ViewModelContentFlags.HiddenContent) != 0)
                {
                    return EmptyTemplate;
                }
                if (node.ModelNode is ViewModelProxyNode)
                {
                    return ExpanderTemplate;
                }
                if (node.Content != null && node.Content.Value is bool)
                {
                    return ReadOnlyBoolTemplate;
                }
                if (node.ModelNode.Content.Value is ExecuteCommand)
                {
                    return ButtonTemplate;
                }

                switch (node.Name)
                {
                    case BuildSettingsPropertiesEnumerator.ScriptPathPropertyName:
                        return ReadOnlyStringTemplate;
                    case BuildSettingsPropertiesEnumerator.BuildDirectoryPropertyName:
                        return PathStringTemplate;
                    case BuildSettingsPropertiesEnumerator.OutputDirectoryPropertyName:
                        return PathStringTemplate;
                    case BuildSettingsPropertiesEnumerator.SourceBaseDirectoryPropertyName:
                        return PathStringTemplate;
                    case BuildSettingsPropertiesEnumerator.MetadataDatabaseDirectoryPropertyName:
                        return PathStringTemplate;
                    case BuildSettingsPropertiesEnumerator.AvailableKeysPropertyName:
                        return MetadataKeyTemplate;
                }
            }
            return null;
        }
    }
}
