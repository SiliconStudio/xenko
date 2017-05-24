// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Defines a custom project template for a <see cref="SolutionPlatform"/>.
    /// </summary>
    [DataContract("SolutionPlatformTemplate")]
    public class SolutionPlatformTemplate
    {
        public SolutionPlatformTemplate(string templatePath, string displayName)
        {
            TemplatePath = templatePath ?? throw new ArgumentNullException(nameof(templatePath));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        }

        /// <summary>
        /// The template path.
        /// </summary>
        public string TemplatePath { get; set; }

        /// <summary>
        /// The display name, which will be shown to user when choosing template.
        /// </summary>
        public string DisplayName { get; set; }
    }
}