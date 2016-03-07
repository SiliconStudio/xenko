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

using System.Windows.Input;

namespace SiliconStudio.Presentation.Drawing
{
    /// <summary>
    /// Specifies functionalities to interact with a <see cref="IDrawingView"/>.
    /// </summary>
    public interface IDrawingController
    {
        /// <summary>
        /// Adds the specified mouse hover manipulator and invokes the <see cref="IManipulatorBase{T}.Started(T)" /> method with the specified mouse event arguments.
        /// </summary>
        /// <param name="view">The plot view.</param>
        /// <param name="manipulator">The manipulator to add.</param>
        /// <param name="args">The event data.</param>
        void AddHoverManipulator(IDrawingView view, IManipulatorBase<MouseEventArgs> manipulator, MouseEventArgs args);

        /// <summary>
        /// Adds the specified mouse manipulator and invokes the <see cref="IManipulatorBase{T}.Started(T)" /> method with the specified mouse event arguments.
        /// </summary>
        /// <param name="view">The plot view.</param>
        /// <param name="manipulator">The manipulator to add.</param>
        /// <param name="args">The event data.</param>
        void AddMouseManipulator(IDrawingView view, IManipulatorBase<MouseEventArgs> manipulator, MouseButtonEventArgs args);
        
        /// <summary>
        /// Binds the command to the key gesture. Removes old bindings to the gesture.
        /// </summary>
        /// <param name="gesture">The key gesture.</param>
        /// <param name="command">The command. If <c>null</c>, the binding will be removed.</param>
        void Bind(KeyGesture gesture, IDrawingViewCommand<KeyEventArgs> command);
        
        /// <summary>
        /// Binds the command to the mouse enter gesture. Removes old bindings to the gesture.
        /// </summary>
        /// <param name="gesture">The mouse enter gesture.</param>
        /// <param name="command">The command. If <c>null</c>, the binding will be removed.</param>
        void Bind(MouseGesture gesture, IDrawingViewCommand<MouseEventArgs> command);

        /// <summary>
        /// Binds the command to the mouse down gesture. Removes old bindings to the gesture.
        /// </summary>
        /// <param name="gesture">The mouse down gesture.</param>
        /// <param name="command">The command. If <c>null</c>, the binding will be removed.</param>
        void Bind(MouseGesture gesture, IDrawingViewCommand<MouseButtonEventArgs> command);
        
        /// <summary>
        /// Binds the command to the mouse wheel gesture. Removes old bindings to the gesture.
        /// </summary>
        /// <param name="gesture">The mouse wheel gesture.</param>
        /// <param name="command">The command. If <c>null</c>, the binding will be removed.</param>
        void Bind(MouseGesture gesture, IDrawingViewCommand<MouseWheelEventArgs> command);

        /// <summary>
        /// Handles key down events.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="args">The event data.</param>
        /// <returns><c>true</c> if the event was handled.</returns>
        bool HandleKeyDown(IDrawingView view, KeyEventArgs args);

        /// <summary>
        /// Handles mouse down events.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="args">The event data.</param>
        /// <returns><c>true</c> if the event was handled.</returns>
        bool HandleMouseDown(IDrawingView view, MouseButtonEventArgs args);

        /// <summary>
        /// Handles mouse enter events.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="args">The event data.</param>
        /// <returns><c>true</c> if the event was handled.</returns>
        bool HandleMouseEnter(IDrawingView view, MouseEventArgs args);

        /// <summary>
        /// Handles mouse leave events.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="args">The event data.</param>
        /// <returns><c>true</c> if the event was handled.</returns>
        bool HandleMouseLeave(IDrawingView view, MouseEventArgs args);

        /// <summary>
        /// Handles mouse move events.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="args">The event data.</param>
        /// <returns><c>true</c> if the event was handled.</returns>
        bool HandleMouseMove(IDrawingView view, MouseEventArgs args);

        /// <summary>
        /// Handles mouse up events.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="args">The event data.</param>
        /// <returns><c>true</c> if the event was handled.</returns>
        bool HandleMouseUp(IDrawingView view, MouseButtonEventArgs args);

        /// <summary>
        /// Handles mouse wheel events.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="args">The event data.</param>
        /// <returns><c>true</c> if the event was handled.</returns>
        bool HandleMouseWheel(IDrawingView view, MouseWheelEventArgs args);

        /// <summary>
        /// Unbinds the gesture.
        /// </summary>
        /// <param name="gesture">The gesture to unbind.</param>
        void Unbind(InputGesture gesture);

        /// <summary>
        /// Unbinds the command from all gestures.
        /// </summary>
        /// <param name="command">The command to unbind.</param>
        void Unbind(IDrawingViewCommand command);

        /// <summary>
        /// Unbinds all commands.
        /// </summary>
        void UnbindAll();
    }
}
