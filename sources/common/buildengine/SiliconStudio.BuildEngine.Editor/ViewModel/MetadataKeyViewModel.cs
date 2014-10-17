using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    public class MetadataKeyViewModel : ViewModelBase
    {
        /// <summary>
        /// Name of the metadata key
        /// </summary>
        public string Name { get { return Key.Name; } }

        /// <summary>
        /// String representation of the type of the metadata key.
        /// </summary>
        public MetadataKey.DatabaseType Type { get { return Key.Type; } }

        public MetadataKey Key { get; private set; }

        public MetadataKeyViewModel(MetadataKey key)
        {
            Key = key;
        }
    }
}