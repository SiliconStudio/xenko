// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL 
using System;
using System.Collections.ObjectModel;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class GraphicsAdapterFactory
    {
        /// <summary>
        /// Gets the adapters.
        /// </summary>
        /// <returns></returns>
        static GraphicsAdapter[] GetAdapters()
        {
            throw new NotImplementedException();
        }
    }
} 
#endif 
