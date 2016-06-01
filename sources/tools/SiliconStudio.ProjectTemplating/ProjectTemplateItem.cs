// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using System.Diagnostics;
using SiliconStudio.Core;

namespace SiliconStudio.ProjectTemplating
{
    /// <summary>
    /// A file item that will be copied by the template.
    /// </summary>
    [DebuggerDisplay("{Source} => {Target}")]
    [DataContract("ProjectTemplateItem")]
    [DataStyle(DataStyle.Compact)]
    public class ProjectTemplateItem
    {
        /// <summary>
        /// Gets or sets the source location relative to the project template.
        /// </summary>
        /// <value>The source location.</value>
        [DataMember(10)]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the target location. If null, the source location is used.
        /// </summary>
        /// <value>The target location.</value>
        [DataMember(20)]
        [DefaultValue(null)]
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this file is a T4 template. Default is false. If SourceLocation has the 
        /// extension '.tt', this is default to true.
        /// </summary>
        /// <value><c>true</c> if this instance is a T4 template; otherwise, <c>false</c>.</value>
        [DataMember(30)]
        [DefaultValue(false)]
        public bool IsTemplate { get; set; }
        
        /// <summary>
        /// Gets or sets a value the name of the current platform.
        /// </summary>
        /// <value>The name of the current platform.</value>
        [DataMember(40)]
        [DefaultValue(null)]
        public string CurrentPlatform { get; set; }

        public override string ToString()
        {
            return string.Format("Source: {0}, Target: {1}, IsTemplate: {2}", Source, Target, IsTemplate);
        }
    }
}
