// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
//
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Compression.cs" company="Matthew Leibowitz">
//   Copyright (c) Matthew Leibowitz
//   This code is licensed under the Apache 2.0 License
//   http://www.apache.org/licenses/LICENSE-2.0.html
// </copyright>
// <summary>
//   Compression method enumeration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System.IO.Compression.Zip
{
    /// <summary>
    /// Compression method enumeration
    /// </summary>
    public enum Compression : ushort
    {
        /// <summary>
        /// Uncompressed storage
        /// </summary> 
        Store = 0, 

        /// <summary>
        /// Deflate compression method
        /// </summary>
        Deflate = 8
    }
}
