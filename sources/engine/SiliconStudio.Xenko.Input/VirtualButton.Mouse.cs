// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
        public class Mouse : VirtualButton
        {
            private Mouse(string name, VirtualButtonType type, int id, bool isPositiveAndNegative = false) : base(name, type, id, isPositiveAndNegative)
            {
            }

            /// <summary>
            /// Equivalent to <see cref="MouseButton.Left"/>.
            /// </summary>
            public static readonly Mouse Left = new Mouse("Mouse.Left", VirtualButtonType.Mouse, 0);

            /// <summary>
            /// Equivalent to <see cref="MouseButton.Middle"/>.
            /// </summary>
            public static readonly Mouse Middle = new Mouse("Mouse.Middle", VirtualButtonType.Mouse, 1);

            /// <summary>
            /// Equivalent to <see cref="MouseButton.Right"/>.
            /// </summary>
            public static readonly Mouse Right = new Mouse("Mouse.Right", VirtualButtonType.Mouse, 2);

            /// <summary>
            /// Equivalent to <see cref="MouseButton.Extended1"/>.
            /// </summary>
            public static readonly Mouse Extended1 = new Mouse("Mouse.Extended1", VirtualButtonType.Mouse, 3);

            /// <summary>
            /// Equivalent to <see cref="MouseButton.Extended2"/>.
            /// </summary>
            public static readonly Mouse Extended2 = new Mouse("Mouse.Extended2", VirtualButtonType.Mouse, 4);

            /// <summary>
            /// Equivalent to X Axis of <see cref="InputManager.MousePosition"/>.
            /// </summary>
            public static readonly Mouse PositionX = new Mouse("Mouse.PositionX", VirtualButtonType.Mouse, 5, true);

            /// <summary>
            /// Equivalent to Y Axis of <see cref="InputManager.MousePosition"/>.
            /// </summary>
            public static readonly Mouse PositionY = new Mouse("Mouse.PositionY", VirtualButtonType.Mouse, 6, true);

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
                    return manager.MousePosition.X;
                }
                else if (index == 6)
                {
                    return manager.MousePosition.Y;
                }

                return 0.0f;
            }
        }
    }
}
