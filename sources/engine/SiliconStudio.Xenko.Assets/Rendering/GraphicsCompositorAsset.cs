// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Assets.Scripts;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    [DataContract("GraphicsCompositorAsset")]
    [Display(85, "Graphics Compositor")]
    [AssetDescription(FileExtension)]
    public class GraphicsCompositorAsset : VisualScriptAsset
    {
        /// <summary>
        /// The default file extension used by the <see cref="GraphicsCompositorAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkgfxcomp";
    }
}