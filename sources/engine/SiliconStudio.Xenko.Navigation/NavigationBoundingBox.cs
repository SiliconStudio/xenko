// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Navigation.Processors;

namespace SiliconStudio.Xenko.Navigation
{
    [DataContract]
    [DefaultEntityComponentProcessor(typeof(BoundingBoxProcessor), ExecutionMode = ExecutionMode.All)]
    [Display("Navigation bounding box")]
    public class NavigationBoundingBox : EntityComponent
    {
    }
}