// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Various <see cref="ProfilingKey"/> used to measure performance across some part of the UI system.
    /// </summary>
    public static class UIProfilerKeys
    {
        public static readonly ProfilingKey UI = new ProfilingKey("UI");

        public static readonly ProfilingKey TouchEventsUpdate = new ProfilingKey(UI, "TouchEvents");
    }
}