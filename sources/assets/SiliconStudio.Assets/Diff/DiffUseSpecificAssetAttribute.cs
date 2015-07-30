// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Base class for <see cref="DiffUseAsset1Attribute"/> and <see cref="DiffUseAsset2Attribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class DiffUseSpecificAssetAttribute : DiffAttributeBase
    {
    }
}