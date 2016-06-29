// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.View;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Windows
{
    /// <summary>
    /// Base class for message-based dialog windows.
    /// </summary>
    public abstract class MessageDialogBase : Window
    {
        /// <summary>
        /// Identifies the <see cref="ButtonsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonsSourceProperty =
            DependencyProperty.Register(nameof(ButtonsSource), typeof(IEnumerable<DialogButtonInfo>), typeof(MessageDialogBase));

        /// <summary>
        /// Identifies the <see cref="MessageTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MessageTemplateProperty =
            DependencyProperty.Register(nameof(MessageTemplate), typeof(DataTemplate), typeof(MessageDialogBase));

        /// <summary>
        /// Identifies the <see cref="MessageTemplateSelector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MessageTemplateSelectorProperty =
            DependencyProperty.Register(nameof(MessageTemplateSelector), typeof(DataTemplateSelector), typeof(MessageDialogBase));

        /// <summary>
        /// Identifies the <see cref="Result"/> dependency property.
        /// </summary>
        protected static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register(nameof(Result), typeof(int), typeof(MessageDialogBase));

        /// <summary>
        /// Identifies the <see cref="ButtonCommand"/> dependency property key.
        /// </summary>
        private static readonly DependencyPropertyKey ButtonCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ButtonCommand), typeof(ICommandBase), typeof(MessageDialogBase), new PropertyMetadata());
        /// <summary>
        /// Identifies the <see cref="ButtonCommand"/> dependency property.
        /// </summary>
        protected static readonly DependencyProperty ButtonCommandProperty = ButtonCommandPropertyKey.DependencyProperty;

        private int result;

        protected MessageDialogBase()
        {
            var serviceProvider = new ViewModelServiceProvider(new[] { new DispatcherService(Dispatcher) });
            ButtonCommand = new AnonymousCommand<int>(serviceProvider, ButtonClick);
        }

        public IEnumerable<DialogButtonInfo> ButtonsSource { get { return (IEnumerable<DialogButtonInfo>)GetValue(ButtonsSourceProperty); } set { SetValue(ButtonsSourceProperty, value); } }

        public DataTemplate MessageTemplate { get { return (DataTemplate)GetValue(MessageTemplateProperty); } set { SetValue(MessageTemplateProperty, value); } }

        public DataTemplateSelector MessageTemplateSelector { get { return (DataTemplateSelector)GetValue(MessageTemplateSelectorProperty); } set { SetValue(MessageTemplateSelectorProperty, value); }} 

        public int Result { get { return (int)GetValue(ResultProperty); } protected set { SetValue(ResultProperty, value); } }

        protected ICommandBase ButtonCommand { get { return (ICommandBase)GetValue(ButtonCommandProperty); } private set { SetValue(ButtonCommandPropertyKey, value); } }

        protected async Task<int> ShowInternal(WindowOwner owner)
        {
            await WindowManager.ShowModal(this, owner);
            return result;
        }

        private void ButtonClick(int parameter)
        {
            result = parameter;
            Close();
        }
    }

}
