// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core
{
    /// <summary>
    /// This class is an implementation of the <see cref="IDisposable"/> interface that does nothing when disposed.
    /// </summary>
    public class NullDisposable : IDisposable
    {
        /// <summary>
        /// A static instance of the <see cref="NullDisposable"/> class.
        /// </summary>
        public static readonly NullDisposable Instance = new NullDisposable();

        /// <summary>
        /// Implementation of the <see cref="IDisposable.Dispose"/> method. This method does nothing.
        /// </summary>
        public void Dispose()
        {
        }
    }
}