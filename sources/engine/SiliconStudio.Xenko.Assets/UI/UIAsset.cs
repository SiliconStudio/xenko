using System;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.UI;
namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// This assets represents a tree of UI elements. 
    /// </summary>
    [DataContract("UIAsset")]
    [AssetDescription(FileExtension)]
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion)]
    [ObjectFactory(typeof(UIFactory))]
    [Display("UI")]
    public class UIAsset : Asset
    {
        private const string CurrentVersion = "1.7.0-alpha01";

        /// <summary>
        /// The default file extension used by the <see cref="UIAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkui";

        /// <summary>
        /// Gets or sets the root UI element.
        /// </summary>
        /// <userdoc>The root UI element.</userdoc>
        [DataMember(10)]
        public object RootElement { get; set; } // FIXME UIElement is not serializable
        //public UIElement RootElement { get; set; }

        private class UIFactory : IObjectFactory
        {
            public object New(Type type)
            {
                return new UIAsset();
            }
        }
    }
}
