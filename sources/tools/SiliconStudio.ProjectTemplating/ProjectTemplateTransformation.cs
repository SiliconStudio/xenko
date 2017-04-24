// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using Microsoft.VisualStudio.TextTemplating;

namespace SiliconStudio.ProjectTemplating
{
    /// <summary>
    /// Base that must be used for all ProjectTemplate
    /// </summary>
    public abstract class ProjectTemplateTransformation : TextTransformation
    {
        /// <summary>
        /// Gets the name of the project.
        /// </summary>
        /// <value>The name of the project.</value>
        public string ProjectName
        {
            get
            {
                return Session["ProjectName"].ToString();
            }
        }

        /// <summary>
        /// Gets the project unique identifier.
        /// </summary>
        /// <value>The project unique identifier.</value>
        public Guid ProjectGuid
        {
            get
            {
                return (Guid)Session["ProjectGuid"];
            }
        }

        /// <summary>
        /// Dynamic properties accessible from the template
        /// </summary>
        public dynamic Properties
        {
            get
            {
                return ((CustomTemplatingSession)Session).Dynamic;
            }
        }
    }
}
