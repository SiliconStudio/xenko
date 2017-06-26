// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.UI.Events;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Provides data for touch input events.
    /// </summary>
    public class TouchEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the time when this event occurred.
        /// </summary>
        public TimeSpan Timestamp { get; internal set; }
        
        /// <summary>
        /// Gets the action that occurred.
        /// </summary>
        public TouchAction Action { get; internal set; }

        /// <summary>
        /// Gets the position of the touch on the screen. Position is normalized between [0,1]. (0,0) is the left top corner, (1,1) is the right bottom corner.
        /// </summary>
        public Vector2 ScreenPosition { get; internal set; }

        /// <summary>
        /// Gets the translation of the touch on the screen since last triggered event (in normalized units). (1,1) represent a translation of the top left corner to the bottom right corner.
        /// </summary>
        public Vector2 ScreenTranslation { get; internal set; }

        /// <summary>
        /// Gets the position of the touch in the UI virtual world space.
        /// </summary>
        public Vector3 WorldPosition { get; internal set; }

        /// <summary>
        /// Gets the translation of the touch in the UI virtual world space.
        /// </summary>
        public Vector3 WorldTranslation { get; internal set; }
    }
}
