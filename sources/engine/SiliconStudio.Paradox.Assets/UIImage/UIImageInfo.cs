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
        /// Gets or sets the size of the non-stretchable borders of the image.
        /// </summary>
        /// <userdoc>
        /// The size in pixels of the non-stretchable parts of the image.
        /// The part sizes are organized as follow: X->Left, Y->Right, Z->Top, W->Bottom.
        /// </userdoc>
        [DataMember(40)]
        public Vector4 Borders { get; set; }
    }
}