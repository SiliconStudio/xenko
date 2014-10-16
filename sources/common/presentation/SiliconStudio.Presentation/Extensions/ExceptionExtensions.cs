// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.Extensions
{
    public static class ExceptionExtensions
    {
        public static void Ignore(this Exception e)
        {
            // Intentionally does nothing.
            // Use this method to suppress warnings related to a catch block doing nothing
        }
    }
}
