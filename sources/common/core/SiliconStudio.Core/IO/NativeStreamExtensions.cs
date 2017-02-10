// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.IO
{
    /// <summary>
    /// Extension methods concerning <see cref="NativeStream"/>.
    /// </summary>
    public static class NativeStreamExtensions
    {
        /// <summary>
        /// Converts a <see cref="Stream"/> to a <see cref="NativeStream"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="stream"/> is already a <see cref="NativeStream"/>, it will be returned as is.
        /// Otherwise, a <see cref="NativeStreamWrapper"/> around it will be created.
        /// </remarks>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        [NotNull]
        public static NativeStream ToNativeStream(this Stream stream)
        {
            var nativeStream = stream as NativeStream;
            if (nativeStream == null)
                nativeStream = new NativeStreamWrapper(stream);

            return nativeStream;
        }
    }
}