using SiliconStudio.Paradox.Graphics.Data;

namespace SiliconStudio.Paradox.UI.Data
{
    public partial class UIImageGroupData : ImageGroupData<UIImageData>
    {
    }

    /// <summary>
    /// Converter type for <see cref="UIImageGroup"/>.
    /// </summary>
    public class UIImageGroupDataConverter : ImageGroupDataConverter<UIImageGroupData, UIImageGroup, UIImageData, UIImage>
    {
    }
}