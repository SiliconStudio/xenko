// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("UIElementLinkComponent")]
    [Display("UI Element Link", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(UIElementLinkProcessor))]
    [ComponentOrder(1600)]
    public sealed class UIElementLinkComponent : EntityComponent
    {
        /// <summary>
        /// Gets or sets the ui component which contains the hierarchy to use.
        /// </summary>
        /// <value>
        /// The ui component which contains the hierarchy to use.
        /// </value>
        /// <userdoc>The reference to the target entity to which to attach the current entity. If null, parent will be used.</userdoc>
        [Display("Target (Parent if not set)")]
        public UIComponent Target { get; set; }

        /// <summary>
        /// Gets or sets the camera component which is required if the UI component is a billboard.
        /// </summary>
        /// <value>
        /// The camera component which is required if the UI component is a billboard.
        /// </value>
        /// <userdoc>The reference to the target camera used to render the component. It is only required in case the parent UI component is a billboard.</userdoc>
        [Display("Camera (if billboard)")]
        public CameraComponent Camera { get; set; }


        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        /// <value>
        /// The name of the element.
        /// </value>
        /// <userdoc>The name of node of the model of the target entity to which attach the current entity.</userdoc>
        public string NodeName { get; set; }
    }
}
