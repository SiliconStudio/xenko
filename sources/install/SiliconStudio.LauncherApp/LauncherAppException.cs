// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.LauncherApp
{
    public class LauncherAppException : Exception
    {
        public LauncherAppException()
        {
        }

        public LauncherAppException(string message) : base(message)
        {
        }

        public LauncherAppException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }

        public LauncherAppException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}