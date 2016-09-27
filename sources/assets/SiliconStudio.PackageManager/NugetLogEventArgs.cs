// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using NuGet;

namespace SiliconStudio.Assets
{
    internal class NugetLogEventArgs : EventArgs
    {
        private readonly MessageLevel level;
        private readonly string message;

        public NugetLogEventArgs(MessageLevel level, string message)
        {
            this.level = level;
            this.message = message;
        }

        public NugetLogEventArgs(MessageLevel level, string message, params object[] args)
        {
            this.level = level;
            this.message = string.Format(message, args);
        }

        public MessageLevel Level
        {
            get
            {
                return level;
            }
        }

        public string Message
        {
            get
            {
                return message;
            }
        }

        public override string ToString()
        {
            return string.Format("Level: {0}, Message: {1}", Level, Message);
        }
    }
}