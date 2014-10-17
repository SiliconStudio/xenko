// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Contains information about a AddReference/Release event.
    /// </summary>
    public class ComponentEventInfo
    {
        public ComponentEventInfo(ComponentEventType type)
        {
            Type = type;
            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {
                StackTrace = ex.StackTrace;
            }

            Time = Environment.TickCount;
        }

        /// <summary>
        /// Gets the event type.
        /// </summary>
        public ComponentEventType Type { get; internal set; }

        /// <summary>
        /// Gets the stack trace at the time of the event.
        /// </summary>
        public string StackTrace { get; internal set; }

        /// <summary>
        /// Gets the time (from Environment.TickCount) at which the event happened.
        /// </summary>
        public int Time { get; internal set; }

        public override string ToString()
        {
            return string.Format("Event Type: [{0}] Time: [{1}]", Type, Time);
        }
    }
}
