using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Assets.UIImage
{
    /// <summary>
    /// Describes information required by an UI image.
    /// </summary>
    [DataContract("UIImageInfo")]
    public class UIImageInfo : ImageInfo
    {
        /// <summary>
        /// Gets or sets the size of the unstretchable borders of the image.
        /// </summary>
        [DataMember(40)]
        public Vector4 Borders { get; set; }
    }
}