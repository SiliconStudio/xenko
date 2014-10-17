using System;
using System.Linq;

using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    public class ObjectMetadataViewModel : ViewModelBase
    {
        private readonly IObjectMetadata metadata;

        /// <inheritdoc/>
        public string ObjectUrl { get { return metadata.ObjectUrl; } }

        /// <inheritdoc/>
        public MetadataKey Key { get { return metadata.Key; }  }

        /// <inheritdoc/>
        public object MetadataValue { get { return metadata.Value; } set { object dummy = null; SetValue(ref dummy, value, () => metadata.Value = value, "MetadataValue"); } }

        private bool updating;

        public ObjectMetadataViewModel(IObjectMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException("metadata");
            this.metadata = metadata;
        }

        protected override void OnPropertyChanged(params string[] propertyNames)
        {
            base.OnPropertyChanged(propertyNames);

            if (updating)
                return;

            if (propertyNames.Contains("MetadataValue"))
            {
                updating = true;
                IObjectMetadata updated = BuildEditionViewModel.GetActiveSession().UpdateMetadataValue(metadata);
                MetadataValue = updated.Value;
                updating = false;
            }
        }
    }
}
