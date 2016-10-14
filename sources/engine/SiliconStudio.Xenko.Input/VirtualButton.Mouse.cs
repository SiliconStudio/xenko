// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Describes a virtual button (a key from a keyboard, a mouse button, an axis of a joystick...etc.).
    /// </summary>
    public partial class VirtualButton
    {
        /// <summary>
        /// Mouse virtual button.
        /// </summary>
        [DataContract("MouseVirtualButton")]
        [Display("Mouse")]
        public class Mouse : VirtualButton
        {
            /// <summary>
            /// Equivalent to <see cref="MouseButton.Left"/>.
            /// </summary>
            public static readonly Mouse Left = new Mouse(VirtualButtonType.Mouse, 0);

            /// <summary>
            /// Equivalent to <see cref="MouseButton.Middle"/>.
            /// </summary>
            public static readonly Mouse Middle = new Mouse(VirtualButtonType.Mouse, 1);

            /// <summary>
            /// Equivalent to <see cref="MouseButton.Right"/>.
            /// </summary>
            public static readonly Mouse Right = new Mouse(VirtualButtonType.Mouse, 2);

            /// <summary>
            /// Equivalent to <see cref="MouseButton.Extended1"/>.
            /// </summary>
            public static readonly Mouse Extended1 = new Mouse(VirtualButtonType.Mouse, 3);

            /// <summary>
            /// Equivalent to <see cref="MouseButton.Extended2"/>.
            /// </summary>
            public static readonly Mouse Extended2 = new Mouse(VirtualButtonType.Mouse, 4);

            /// <summary>
            /// Equivalent to X Axis of <see cref="InputManager.MousePosition"/>.
            /// </summary>
            public static readonly Mouse PositionX = new Mouse(VirtualButtonType.Mouse, 5, true);

            /// <summary>
            /// Equivalent to Y Axis of <see cref="InputManager.MousePosition"/>.
            /// </summary>
            public static readonly Mouse PositionY = new Mouse(VirtualButtonType.Mouse, 6, true);


            /// <summary>
            /// Equivalent to <see cref="InputManager.MouseWheelDelta"/>.
            /// </summary>
            public static readonly Mouse Wheel = new Mouse(VirtualButtonType.Mouse, 7, true);

            public Mouse(VirtualButtonType type, int id, bool isPositiveAndNegative = false) : base(type, id)
            {
            }

            public override string Name
            {
                get
                {
                    int id2 = Id ^ (int)VirtualButtonType.Mouse;
                    if(id2 < 5)
                        return $"Mouse.{(MouseButton)id2}";
                    return $"Mouse.Axis.{(MouseAxis)(id2-5)}";
                }
            }

            public override float GetValue(InputManager manager)
            {
                int index = (Id & TypeIdMask);
                if (index < 5)
                {
                    if (manager.IsMouseButtonDown((MouseButton)index))
                    {
                        return 1.0f;
                    }
                }
                else if (index == 5)
                {
                    return manager.MouseDelta.X;
                }
                else if (index == 6)
                {
                    return manager.MouseDelta.Y;
                }
                else if (index == 7)
                {
                    return manager.MouseWheelDelta;
                }

                return 0.0f;
            }
        }
    }
}
