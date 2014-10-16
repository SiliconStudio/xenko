// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Effects.Data
{
    /// <summary>
    /// Base class to store additional information for NodeData.
    /// </summary>
    [ContentSerializer(typeof(DataContentSerializer<NodePropertyData>))]
    public class NodePropertyData : ContentData
    {
    }
}