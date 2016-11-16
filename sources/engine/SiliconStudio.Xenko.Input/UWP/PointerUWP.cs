// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_UWP
using System;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using SiliconStudio.Core.Mathematics;
using Point = Windows.Foundation.Point;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// UWP Mouse device
    /// </summary>
    public class PointerUWP : PointerDeviceBase
    {
        private readonly FrameworkElement uiControl;

        public PointerUWP(FrameworkElement uiControl)
        {
            this.uiControl = uiControl;

            uiControl.SizeChanged += UIControlOnSizeChanged;
            uiControl.PointerMoved += (sender, args) => HandlePointer(InputEventType.Moved, args);
            uiControl.PointerPressed += (sender, args) => HandlePointer(InputEventType.Down, args);
            uiControl.PointerReleased += (sender, args) => HandlePointer(InputEventType.Up, args);
            uiControl.PointerExited += (sender, args) => HandlePointer(InputEventType.Out, args);
            uiControl.PointerCanceled += (sender, args) => HandlePointer(InputEventType.Canceled, args);
            uiControl.PointerCaptureLost += (sender, args) => HandlePointer(InputEventType.Canceled, args);

            // Set initial surface size
            SetSurfaceSize(new Vector2((float)uiControl.Width, (float)uiControl.Height));
        }

        public override string DeviceName { get; } = "UWP Pointer";
        public override Guid Id { get; } = new Guid("9b1e36b6-de69-4313-89dd-7cbfbe1a436e");
        public override PointerType Type { get; } = PointerType.Unknown;
        
        protected virtual void HandlePointer(InputEventType type, PointerRoutedEventArgs args)
        {
            var pointer = args.Pointer;
            var point = args.GetCurrentPoint(uiControl);
            if (point.PointerDevice.PointerDeviceType == PointerDeviceType.Mouse)
            {
                PointerInputEvents.Add(new PointerInputEvent
                {
                    Id = (int)pointer.PointerId,
                    Position = PointToVector2(point.Position),
                    Type = type
                });
            }
        }
        private void UIControlOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var newSize = sizeChangedEventArgs.NewSize;
            SetSurfaceSize(new Vector2((float)newSize.Width, (float)newSize.Height));
        }
        private Vector2 PointToVector2(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
    }
}
#endif