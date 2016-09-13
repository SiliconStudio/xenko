using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    [DataContract("ProjectSourceCodeAsset")]
    public abstract class ProjectSourceCodeAsset : SourceCodeAsset, IProjectAsset
    {
        /// <inheritdoc/>
        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string AbsoluteProjectLocation { get; set; }

        /// <inheritdoc/>
        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string ProjectInclude { get; set; }

        /// <inheritdoc/>
        [DataMember(Mask = DataMemberAttribute.IgnoreMask)]
        [Display(Browsable = false)]
        public string ProjectName { get; set; }
    }
}
