// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    /// <summary>
    /// Implements IViewModelContent for a given object.
    /// </summary>
    public class RootViewModelContent : ObjectContent
    {
        public RootViewModelContent(object value, Type type = null)
            : base(value, type ?? value.GetType(), null)
        {
        }
    }
}