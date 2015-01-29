using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// Represent of group of <see cref="UIImage"/>
    /// </summary>
    [DataSerializerGlobal(typeof(ReferenceSerializer<UIImageGroup>), Profile = "Asset")]
    [ContentSerializer(typeof(DataContentSerializer<UIImageGroup>))]
    public class UIImageGroup : ImageGroup<UIImage>
    {
    }
}