// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls.Commands
{
    /// <summary>
    /// This class provides an instance of all commands in the namespace <see cref="SiliconStudio.Presentation.Controls.Commands"/>.
    /// These instances can be used in XAML with the <see cref="StaticExtension"/> markup extension.
    /// </summary>
    public static class ControlCommands
    {
        /// <summary>
        /// Initialize the static properties of the <see cref="ControlCommands"/> class.
        /// </summary>
        static ControlCommands()
        {
            ClearSelectionCommand = new RoutedCommand("ClearSelectionCommand", typeof(Selector));
            CommandManager.RegisterClassCommandBinding(typeof(Selector), new CommandBinding(ClearSelectionCommand, OnClearSelectionCommand));
            SetAllVector3Components = new RoutedCommand("SetAllVector3Components", typeof(Vector3Editor));
            CommandManager.RegisterClassCommandBinding(typeof(Vector3Editor), new CommandBinding(SetAllVector3Components, OnSetAllVector3Components));
        }

        /// <summary>
        /// Clears the current selection of a text box.
        /// </summary>
        public static RoutedCommand ClearSelectionCommand { get; private set; }

        /// <summary>
        /// Sets all the component of a <see cref="Vector3Editor"/> to the value given as parameter.
        /// </summary>
        public static RoutedCommand SetAllVector3Components { get; private set; }

        private static void OnClearSelectionCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var selector = sender as Selector;
            if (selector != null)
            {
                selector.SelectedItem = null;
            }
        }
        
        private static void OnSetAllVector3Components(object sender, ExecutedRoutedEventArgs e)
        {
            var vectorEditor = sender as Vector3Editor;
            if (vectorEditor != null)
            {
                try
                {
                    var value = Convert.ToSingle(e.Parameter);
                    vectorEditor.SetCurrentValue(Vector3Editor.XProperty, value);
                    vectorEditor.SetCurrentValue(Vector3Editor.YProperty, value);
                    vectorEditor.SetCurrentValue(Vector3Editor.ZProperty, value);
                }
                catch (Exception ex)
                {
                    ex.Ignore();
                }
            }
        }
    }
}
