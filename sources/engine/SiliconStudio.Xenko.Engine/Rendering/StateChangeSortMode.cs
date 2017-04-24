// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Sort elements according to the pattern: [RenderFeature Sort Key 8 bits] RenderObject states 32 bits] [Distance front to back 16 bits]
    /// </summary>
    [DataContract("SortModeStateChange")]
    public class StateChangeSortMode : SortModeDistance
    {
        public StateChangeSortMode() : base(false)
        {
            statePosition = 32;
            distancePosition = 0;
        }
    }
}
