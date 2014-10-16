// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// A registry containing the thumbnail compilers of the assets.
    /// </summary>
    public class ThumbnailCompilerRegistry : AttributeBasedRegistry<ThumbnailCompilerAttribute, IAssetCompiler>
    {

    }
}