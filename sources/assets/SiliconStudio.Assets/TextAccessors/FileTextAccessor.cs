using SiliconStudio.Core;

namespace SiliconStudio.Assets.TextAccessors
{
    [DataContract]
    public class FileTextAccessor : ISerializableTextAccessor
    {
        [DataMember]
        public string FilePath { get; set; }

        public ITextAccessor Create()
        {
            return new DefaultTextAccessor { FilePath = FilePath };
        }
    }
}