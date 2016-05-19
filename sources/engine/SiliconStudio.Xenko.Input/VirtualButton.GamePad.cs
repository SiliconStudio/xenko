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
        /// GamePad virtual button.
        /// </summary>
        public class GamePad : VirtualButton
        {
            private GamePad(string name, VirtualButtonType type, int id, bool isPositiveAndNegative = false) : base(name, type, id, isPositiveAndNegative)
            {
                PadIndex = -1;
            }

            private GamePad(GamePad parentPad, int index) : base(parentPad.Name, parentPad.Type, parentPad.Id, parentPad.IsPositiveAndNegative)
            {
                PadIndex = index;
            }

            /// <summary>
            /// The pad index.
            /// </summary>
            public readonly int PadIndex;

            /// <summary>
            /// Return an instance of a particular GamePad.
            /// </summary>
            /// <param name="index">The gamepad index.</param>
            /// <returns>A new GamePad button linked to the gamepad index.</returns>
            public GamePad Index(int index)
            {
                return new GamePad(this, index);
            }

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.PadUp"/>.
            /// </summary>
            public static readonly GamePad PadUp = new GamePad("GamePad.PadUp", VirtualButtonType.GamePad, 0);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.PadDown"/>.
            /// </summary>
            public static readonly GamePad PadDown = new GamePad("GamePad.PadDown", VirtualButtonType.GamePad, 1);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.PadLeft"/>.
            /// </summary>
            public static readonly GamePad PadLeft = new GamePad("GamePad.PadLeft", VirtualButtonType.GamePad, 2);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.PadRight"/>.
            /// </summary>
            public static readonly GamePad PadRight = new GamePad("GamePad.PadRight", VirtualButtonType.GamePad, 3);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.Start"/>.
            /// </summary>
            public static readonly GamePad Start = new GamePad("GamePad.Start", VirtualButtonType.GamePad, 4);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.Back"/>.
            /// </summary>
            public static readonly GamePad Back = new GamePad("GamePad.Back", VirtualButtonType.GamePad, 5);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.LeftThumb"/>.
            /// </summary>
            public static readonly GamePad LeftThumb = new GamePad("GamePad.LeftThumb", VirtualButtonType.GamePad, 6);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.RightThumb"/>.
            /// </summary>
            public static readonly GamePad RightThumb = new GamePad("GamePad.RightThumb", VirtualButtonType.GamePad, 7);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.LeftShoulder"/>.
            /// </summary>
            public static readonly GamePad LeftShoulder = new GamePad("GamePad.LeftShoulder", VirtualButtonType.GamePad, 8);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.RightShoulder"/>.
            /// </summary>
            public static readonly GamePad RightShoulder = new GamePad("GamePad.RightShoulder", VirtualButtonType.GamePad, 9);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.A"/>.
            /// </summary>
            public static readonly GamePad A = new GamePad("GamePad.A", VirtualButtonType.GamePad, 12);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.B"/>.
            /// </summary>
            public static readonly GamePad B = new GamePad("GamePad.B", VirtualButtonType.GamePad, 13);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.X"/>.
            /// </summary>
            public static readonly GamePad X = new GamePad("GamePad.X", VirtualButtonType.GamePad, 14);

            /// <summary>
            /// Equivalent to <see cref="GamePadButton.Y"/>.
            /// </summary>
            public static readonly GamePad Y = new GamePad("GamePad.Y", VirtualButtonType.GamePad, 15);

            /// <summary>
            /// Equivalent to the X Axis of <see cref="GamePadState.LeftThumb"/>.
            /// </summary>
            public readonly static GamePad LeftThumbAxisX = new GamePad("GamePad.LeftThumbAxisX", VirtualButtonType.GamePad, 16, true);

            /// <summary>
            /// Equivalent to the Y Axis of <see cref="GamePadState.LeftThumb"/>.
            /// </summary>
            public readonly static GamePad LeftThumbAxisY = new GamePad("GamePad.LeftThumbAxisY", VirtualButtonType.GamePad, 17, true);

            /// <summary>
            /// Equivalent to the X Axis of <see cref="GamePadState.RightThumb"/>.
            /// </summary>
            public readonly static GamePad RightThumbAxisX = new GamePad("GamePad.RightThumbAxisX", VirtualButtonType.GamePad, 18, true);

            /// <summary>
            /// Equivalent to the Y Axis of <see cref="GamePadState.RightThumb"/>.
            /// </summary>
            public readonly static GamePad RightThumbAxisY = new GamePad("GamePad.RightThumbAxisY", VirtualButtonType.GamePad, 19, true);

            /// <summary>
            /// Equivalent to <see cref="GamePadState.LeftTrigger"/>.
            /// </summary>
            public readonly static GamePad LeftTrigger = new GamePad("GamePad.LeftTrigger", VirtualButtonType.GamePad, 20);

            /// <summary>
            /// Equivalent to <see cref="GamePadState.RightTrigger"/>.
            /// </summary>
            public readonly static GamePad RightTrigger = new GamePad("GamePad.RightTrigger", VirtualButtonType.GamePad, 21);


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
