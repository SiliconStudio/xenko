using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    [DataContract("ProjectSourceCodeAsset")]
    public abstract class ProjectSourceCodeAsset : SourceCodeAsset
    {
        /// <summary>
        /// Gets or sets the absolute project (csproj) location of this asset on the disk.
        /// </summary>
        /// <value>The absolute source location.</value>
        [Display(Browsable = false)]
        public string AbsoluteProjectLocation { get; set; }

        [Display(Browsable = false)]
        public string ProjectInclude { get; set; }

        [Display(Browsable = false)]
        public string ProjectName { get; set; }
    }
}
