using SiliconStudio.Core;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Assets
{
    [DataContract]
    [Display("Editor Settings")]
    public class EditorSettings : Configuration
    {
        public EditorSettings()
        {
            OfflineOnly = true;
        }

        /// <userdoc>
        /// This setting applies to thumbnails and asset previews
        /// </userdoc>
        [DataMember(0)]
        public RenderingMode RenderingMode = RenderingMode.HDR;
    }
}