// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Navigation.Processors;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// A three dimensional bounding box  using the scale of the owning entity as the box extent. This is used to limit the area in which navigation meshes are generated
    /// </summary>
    [DataContract]
    [DefaultEntityComponentProcessor(typeof(BoundingBoxProcessor), ExecutionMode = ExecutionMode.All)]
    [Display("Navigation bounding box")]
    public class NavigationBoundingBoxComponent : EntityComponent
    {
        /// <summary>
        /// The size of one edge of the bounding box
        /// </summary>
        [DataMember(0)]
        public Vector3 Size;
    }
}
