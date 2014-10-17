using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// Represent of group of <see cref="UIImage"/>
    /// </summary>
    [DataConverter(AutoGenerate = false, ContentReference = true)]
    public class UIImageGroup : ImageGroup<UIImage>
    {
    }
}