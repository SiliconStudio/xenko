// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// The class represents the argument of the <see cref="ItemListCompiler.AssetCompiled"/> event raised by the <see cref="ItemListCompiler"/> class.
    /// </summary>
    public class AssetCompiledArgs : EventArgs
    {
        /// <summary>
        /// Constructs an <see cref="AssetCompiledArgs"/> instance.
        /// </summary>
        /// <param name="asset">The asset that has been compiled. Cannot be null.</param>
        /// <param name="result">The result of the asset compilation. Cannot be null.</param>
        public AssetCompiledArgs(AssetItem asset, AssetCompilerResult result)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            if (result == null) throw new ArgumentNullException("result");
            Asset = asset;
            Result = result;
        }

        /// <summary>
        /// The asset item that has just been compiled.
        /// </summary>
        public AssetItem Asset { get; set; }

        /// <summary>
        /// The result of the asset compilation.
        /// </summary>
        public AssetCompilerResult Result { get; set; }
    }
}
