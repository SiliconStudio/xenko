// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

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
        }

        /// <summary>
        /// Clears the current selection of a text box.
        /// </summary>
        public static RoutedCommand ClearSelectionCommand { get; private set; }

        private static void OnClearSelectionCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var selector = sender as Selector;
            if (selector != null)
            {
                selector.SelectedItem = null;
            }
        }
    }
}
