using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    // I'm forced to do these class by design. This really sucks, I've started a discussion on AvalonDock codeplex page.
    public abstract class AnchorableViewModel : ViewModelBase
    {
        public string Title { get{ return title; } set { SetValue(ref title, value, "Title"); } }
        private string title;

        public string DefaultPane { get; protected set; }

        public BuildEditionViewModel Edition { get { return edition; } set { SetValue(ref edition, value, "Edition"); } }
        private BuildEditionViewModel edition;

        public bool IsVisible { get { return isVisible; } set { SetValue(ref isVisible, value, "IsVisible"); } }
        private bool isVisible;

        protected AnchorableViewModel(BuildEditionViewModel edition)
        {
            Edition = edition;
            IsVisible = true;
        }
    }

    public class ToolboxViewModel : AnchorableViewModel
    {
        public ToolboxViewModel(BuildEditionViewModel edition)
            : base(edition)
        {
            Title = "Toolbox";
            DefaultPane = "DefaultToolboxPane";
        }
    }

    public class PropertyGridViewModel : AnchorableViewModel
    {
        public PropertyGridViewModel(BuildEditionViewModel edition)
            : base(edition)
        {
            Title = "Properties";
            DefaultPane = "DefaultPropertiesPane";
        }
    }
}
