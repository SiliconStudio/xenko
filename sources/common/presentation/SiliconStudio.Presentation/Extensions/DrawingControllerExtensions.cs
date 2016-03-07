// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#region Copyright and license
// Some parts of this file were inspired by OxyPlot (https://github.com/oxyplot/oxyplot)
/*
The MIT license (MTI)
https://opensource.org/licenses/MIT

Copyright (c) 2014 OxyPlot contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Windows.Input;
using SiliconStudio.Presentation.Drawing;

namespace SiliconStudio.Presentation.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="IDrawingController" />.
    /// </summary>
    public static class DrawingControllerExtensions
    {
        /// <summary>
        /// Binds the key to the command.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="key">The key.</param>
        /// <param name="command">A command that takes key event arguments.</param>
        public static void BindKeyDown(this IDrawingController controller, Key key, IDrawingViewCommand<KeyEventArgs> command)
        {
            if (key == Key.None) throw new ArgumentException($"The value cannot be {Key.None}", nameof(key));
            controller.Bind(new KeyGesture(key), command);
        }

        /// <summary>
        /// Binds the modifier+key to the command.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="key">The key.</param>
        /// <param name="modifiers">The key modifiers.</param>
        /// <param name="command">A command that takes key event arguments.</param>
        public static void BindKeyDown(this IDrawingController controller, Key key, ModifierKeys modifiers, IDrawingViewCommand<KeyEventArgs> command)
        {
            if (key == Key.None) throw new ArgumentException($"The value cannot be {Key.None}", nameof(key));
            controller.Bind(new KeyGesture(key, modifiers), command);
        }

        /// <summary>
        /// Binds the mouse action to the command.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="mouseAction">The mouse action.</param>
        /// <param name="command">A command that takes mouse event arguments.</param>
        public static void BindMouseDown(this IDrawingController controller, MouseAction mouseAction, IDrawingViewCommand<MouseButtonEventArgs> command)
        {
            switch (mouseAction)
            {
                case MouseAction.None:
                    throw new ArgumentException($"The value cannot be {MouseAction.None}", nameof(mouseAction));
                case MouseAction.WheelClick:
                    throw new ArgumentException($"The value cannot be {MouseAction.WheelClick}", nameof(mouseAction));

                default:
                    controller.Bind(new MouseGesture(mouseAction), command);
                    break;
            }
        }

        /// <summary>
        /// Binds the modifier+mouse action gesture to the command.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="mouseAction">The mouse action.</param>
        /// <param name="modifiers">The modifiers.</param>
        /// <param name="command">A command that takes mouse event arguments.</param>
        public static void BindMouseDown(this IDrawingController controller, MouseAction mouseAction, ModifierKeys modifiers, IDrawingViewCommand<MouseButtonEventArgs> command)
        {
            switch (mouseAction)
            {
                case MouseAction.None:
                    throw new ArgumentException($"The value cannot be {MouseAction.None}", nameof(mouseAction));
                case MouseAction.WheelClick:
                    throw new ArgumentException($"The value cannot be {MouseAction.WheelClick}", nameof(mouseAction));

                default:
                    controller.Bind(new MouseGesture(mouseAction, modifiers), command);
                    break;
            }
        }

        /// <summary>
        /// Binds the mouse enter event to the specified command.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="command">A command that takes mouse event arguments.</param>
        public static void BindMouseEnter(this IDrawingController controller, IDrawingViewCommand<MouseEventArgs> command)
        {
            controller.Bind(new MouseGesture(MouseAction.None), command);
        }

        /// <summary>
        /// Binds the mouse wheel event to the specified command.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="command">A command that takes mouse wheel event arguments.</param>
        public static void BindMouseWheel(this IDrawingController controller, IDrawingViewCommand<MouseWheelEventArgs> command)
        {
            controller.Bind(new MouseGesture(MouseAction.WheelClick), command);
        }

        /// <summary>
        /// Binds the modifier+mouse wheel event to the specified command.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="modifiers">The modifier key(s).</param>
        /// <param name="command">A command that takes mouse wheel event arguments.</param>
        public static void BindMouseWheel(this IDrawingController controller, ModifierKeys modifiers, IDrawingViewCommand<MouseWheelEventArgs> command)
        {
            controller.Bind(new MouseGesture(MouseAction.WheelClick, modifiers), command);
        }

        /// <summary>
        /// Unbinds the specified mouse down gesture.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="mouseAction">The mouse button.</param>
        /// <param name="modifiers">The modifier keys.</param>
        public static void UnbindMouseDown(this IDrawingController controller, MouseAction mouseAction, ModifierKeys modifiers = ModifierKeys.None)
        {
            switch (mouseAction)
            {
                case MouseAction.None:
                    throw new ArgumentException($"The value cannot be {MouseAction.None}", nameof(mouseAction));
                case MouseAction.WheelClick:
                    throw new ArgumentException($"The value cannot be {MouseAction.WheelClick}", nameof(mouseAction));

                default:
                    controller.Unbind(new MouseGesture(mouseAction, modifiers));
                    break;
            }
        }

        /// <summary>
        /// Unbinds the specified key down gesture.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="key">The key.</param>
        /// <param name="modifiers">The modifier keys.</param>
        public static void UnbindKeyDown(this IDrawingController controller, Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            if (key == Key.None) throw new ArgumentException($"The value cannot be {Key.None}", nameof(key));
            controller.Unbind(new KeyGesture(key, modifiers));
        }

        /// <summary>
        /// Unbinds the mouse enter gesture.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public static void UnbindMouseEnter(this IDrawingController controller)
        {
            controller.Unbind(new MouseGesture(MouseAction.None));
        }

        /// <summary>
        /// Unbinds the mouse wheel gesture.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public static void UnbindMouseWheel(this IDrawingController controller)
        {
            controller.Unbind(new MouseGesture(MouseAction.WheelClick));
        }
    }
}
