// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.UI;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Add an <see cref="UIElement"/> to an <see cref="Entity"/>.
    /// </summary>
    [DataContract("UIComponent")]
    [Display(98, "UI")]
    [DefaultEntityComponentRenderer(typeof(UIComponentRenderer))]
    [DefaultEntityComponentProcessor(typeof(UIComponentProcessor))]
    public class UIComponent : EntityComponent
    {
        public static PropertyKey<UIComponent> Key = new PropertyKey<UIComponent>("Key", typeof(UIComponent));

        /// <summary>
        /// Gets or sets the root element of the UI hierarchy.
        /// </summary>
        /// <userdoc>The root element of the UI hierarchy.</userdoc>
        [DataMember(5)]
        [Display("Root Element")]
        [DataMemberIgnore] // TODO this is temporary as long as we don't have an UI editor and UI data asset.
        public UIElement RootElement { get; set; }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}