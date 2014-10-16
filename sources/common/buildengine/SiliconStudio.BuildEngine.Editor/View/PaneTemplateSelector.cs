using System.Windows;
using System.Windows.Controls;

using SiliconStudio.BuildEngine.Editor.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.View
{
    public class PaneTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BuildStepTemplate { get; set; }
        public DataTemplate ToolboxTemplate { get; set; }
        public DataTemplate PropertiesTemplate { get; set; }
        public DataTemplate BuildSettingsTemplate { get; set; }
        public DataTemplate FileExplorerTemplate { get; set; }
        public DataTemplate AssetExplorerTemplate { get; set; }
        public DataTemplate MetadataTemplate { get; set; }
        
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is BuildSessionViewModel)
                return BuildStepTemplate;

            if (item is ToolboxViewModel)
                return ToolboxTemplate;

            if (item is PropertyGridViewModel)
                return PropertiesTemplate;

            if (item is BuildSettingsViewModel)
                return BuildSettingsTemplate;

            if (item is FileExplorerViewModel)
                return FileExplorerTemplate;

            if (item is AssetExplorerViewModel)
                return AssetExplorerTemplate;

            if (item is MetadataViewModel)
                return MetadataTemplate;

            return null;
        }
    }
}