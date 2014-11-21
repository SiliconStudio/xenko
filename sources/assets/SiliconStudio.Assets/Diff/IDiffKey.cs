// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Provides an custom equality comparer to match items in <see cref="AssetDiff.DiffCollection"/>.
    /// </summary>
    public interface IDiffKey
    {
        object GetDiffKey();
    }
}