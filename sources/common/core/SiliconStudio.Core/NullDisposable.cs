// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
