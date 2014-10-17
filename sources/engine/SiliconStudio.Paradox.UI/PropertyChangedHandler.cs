// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.UI
{
    public delegate void PropertyChangedHandler<T>(Object sender, PropertyChangedArgs<T> e);
}