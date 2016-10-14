// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Describes a virtual button (a key from a keyboard, a mouse button, an axis of a joystick...etc.).
    /// </summary>
    public partial class VirtualButton
    {
        /// <summary>
        /// GamePad virtual button.
        /// </summary>
        [DataContract("GamepadVirtualButton")]
        [Display("Gamepad")]
        public class GamePad : VirtualButton
        {
            /// <summary>
            /// Equivalent to <see cref="GamePadButton.PadUp"/>.
            /// </summary>
            public static readonly GamePad PadUp = new GamePad(VirtualButtonType.GamePad, 0);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.PadDown"/>.
            /// </summary>
            public static readonly GamePad PadDown = new GamePad(VirtualButtonType.GamePad, 1);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.PadLeft"/>.
            /// </summary>
            public static readonly GamePad PadLeft = new GamePad(VirtualButtonType.GamePad, 2);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.PadRight"/>.
            /// </summary>
            public static readonly GamePad PadRight = new GamePad(VirtualButtonType.GamePad, 3);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.Start"/>.
            /// </summary>
            public static readonly GamePad Start = new GamePad(VirtualButtonType.GamePad, 4);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.Back"/>.
            /// </summary>
            public static readonly GamePad Back = new GamePad(VirtualButtonType.GamePad, 5);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.LeftThumb"/>.
            /// </summary>
            public static readonly GamePad LeftThumb = new GamePad(VirtualButtonType.GamePad, 6);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.RightThumb"/>.
            /// </summary>
            public static readonly GamePad RightThumb = new GamePad(VirtualButtonType.GamePad, 7);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.LeftShoulder"/>.
            /// </summary>
            public static readonly GamePad LeftShoulder = new GamePad(VirtualButtonType.GamePad, 8);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.RightShoulder"/>.
            /// </summary>
            public static readonly GamePad RightShoulder = new GamePad(VirtualButtonType.GamePad, 9);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.A"/>.
            /// </summary>
            public static readonly GamePad A = new GamePad(VirtualButtonType.GamePad, 12);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.B"/>.
            /// </summary>
            public static readonly GamePad B = new GamePad(VirtualButtonType.GamePad, 13);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.X"/>.
            /// </summary>
            public static readonly GamePad X = new GamePad(VirtualButtonType.GamePad, 14);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.Y"/>.
            /// </summary>
            public static readonly GamePad Y = new GamePad(VirtualButtonType.GamePad, 15);

            /// <summary>
            /// Equivalent to the X Axis of <see cref="GamePadState.LeftThumb"/>.
            /// </summary>
            public static readonly GamePad LeftThumbAxisX = new GamePad(VirtualButtonType.GamePad, 16, true);

            /// <summary>
            /// Equivalent to the Y Axis of <see cref="GamePadState.LeftThumb"/>.
            /// </summary>
            public static readonly GamePad LeftThumbAxisY = new GamePad(VirtualButtonType.GamePad, 17, true);

            /// <summary>
            /// Equivalent to the X Axis of <see cref="GamePadState.RightThumb"/>.
            /// </summary>
            public static readonly GamePad RightThumbAxisX = new GamePad(VirtualButtonType.GamePad, 18, true);

            /// <summary>
            /// Equivalent to the Y Axis of <see cref="GamePadState.RightThumb"/>.
            /// </summary>
            public static readonly GamePad RightThumbAxisY = new GamePad(VirtualButtonType.GamePad, 19, true);

            /// <summary>
            /// Equivalent to <see cref="GamePadState.LeftTrigger"/>.
            /// </summary>
            public static readonly GamePad LeftTrigger = new GamePad(VirtualButtonType.GamePad, 20);

            /// <summary>
            /// Equivalent to <see cref="GamePadState.RightTrigger"/>.
            /// </summary>
            public static readonly GamePad RightTrigger = new GamePad(VirtualButtonType.GamePad, 21);

            /// <summary>
            /// The pad index.
            /// </summary>
            public readonly int PadIndex;

            public GamePad(VirtualButtonType type, int id, bool isPositiveAndNegative = false) : base(type, id)
            {
                PadIndex = -1;
            }

            public GamePad(GamePad parentPad, int index) : base(parentPad.Type, parentPad.Id)
            {
                PadIndex = index;
            }

            public override string Name
            {
                get
                {
                    int id2 = Id ^ (int)VirtualButtonType.GamePad;
                    if(id2 <= 15)
                        return $"GamePad.{(GamePadButtonSingle)id2}";
                    return $"GamePad.Axis.{(GamePadAxis)(id2-16)}";
                }
            }

            /// <summary>
            /// Return an instance of a particular GamePad.
            /// </summary>
            /// <param name="index">The gamepad index.</param>
            /// <returns>A new GamePad button linked to the gamepad index.</returns>
            public GamePad Index(int index)
            {
                return new GamePad(this, index);
            }

            public override float GetValue(InputManager manager)
            {
                var state = manager.GetGamePad(PadIndex);

                var index = (int)(Id & TypeIdMask);

                if (index <= 15)
                {
                    if ((state.Buttons & (GamePadButton)(1 << index)) != 0)
                    {
                        return 1.0f;
                    }
                }
                else
                {
                    switch (index)
                    {
                        case 16:
                            return state.LeftThumb.X;
                        case 17:
                            return state.LeftThumb.Y;
                        case 18:
                            return state.RightThumb.X;
                        case 19:
                            return state.RightThumb.Y;
                        case 20:
                            return state.LeftTrigger;
                        case 21:
                            return state.RightTrigger;
                    }
                }

                return 0.0f;
            }
        }
    }
}
