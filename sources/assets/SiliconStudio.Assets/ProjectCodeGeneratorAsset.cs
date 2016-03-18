using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    [DataContract("ProjectCodeGeneratorAsset")]
    public abstract class ProjectCodeGeneratorAsset : ProjectSourceCodeAsset
    {
        [Display(Browsable = false)]
        public abstract string Generator { get; set; }

        [Display(Browsable = false)]
        public string GeneratedAbsolutePath { get; set; }

        [Display(Browsable = false)]
        public string GeneratedInclude { get; set; }
    }
}