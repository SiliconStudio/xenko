using System;
using System.ComponentModel;
using System.Windows.Input;

using SiliconStudio.Presentation.Quantum.Legacy;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Legacy;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    public class BuildSettingsViewModel : AnchorableViewModel
    {
        public ObservableViewModelNode SettingsRootNode { get { return (ObservableViewModelNode)contextUI.Root; } }

        private readonly ViewModelContext context;
        private readonly ViewModelContext contextUI = new ViewModelContext();
        private readonly ViewModelState state = new ViewModelState();

        public BuildSettingsViewModel(BuildEditionViewModel edition)
            : base(edition)
        {
            Title = "Build settings";
            DefaultPane = "DefaultPropertiesPane";

            context = new ViewModelContext(edition.GuidContainer);
            context.ChildrenPropertyEnumerators.Add(new BuildSettingsPropertiesEnumerator());

            edition.PropertyChanged += BuildEditionPropertyChanged;
            Refresh();
        }

        private void BuildEditionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveSession")
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            IViewModelNode node = null;
            if (Edition.ActiveSession != null)
            {
                Guid guid = context.GetOrCreateGuid(Edition.ActiveSession);
                if (!context.ViewModelByGuid.TryGetValue(guid, out node))
                {
                    node = new ViewModelNode("Settings", new ObjectContent(Edition.ActiveSession, typeof(BuildSessionViewModel), null));
                    context.RegisterViewModel(node);
                    node.GenerateChildren(context);
                }
            }
            context.Root = node;

            OnPropertyChanging("SettingsRootNode");
            ObservableViewModelNode.ForceRefresh(contextUI, context, state);
            OnPropertyChanged("SettingsRootNode");
        }
    }
}
