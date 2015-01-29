// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

using SiliconStudio.Core;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.Extensions;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Threading;

namespace SiliconStudio.Presentation.Behaviors
{
    [Obsolete("This behavior is obsolete.")]
    public class RegisterKeyBindingBehavior : Behavior<FrameworkElement>
    {
        public KeyGesture KeyGesture
        {
            get { return (KeyGesture)GetValue(KeyGestureProperty); }
            set { SetValue(KeyGestureProperty, value); }
        }

        public static readonly DependencyProperty KeyGestureProperty = DependencyProperty.Register(
            "KeyGesture",
            typeof(KeyGesture),
            typeof(RegisterKeyBindingBehavior));

        private IDisposable bindingRegistration;

        protected override void OnAttached()
        {
            if ((AssociatedObject is ICommandSource) == false)
            {
                var message = string.Format("The Host of a '{0}' must be of type '{1}'",
                    typeof(RegisterKeyBindingBehavior).Name,
                    typeof(ICommandSource).FullName);
                throw new InvalidOperationException(message);
            }

            SpawnCommandBindingResolutionCheckThread();
        }

        private void SpawnCommandBindingResolutionCheckThread()
        {
            // Warning: a very bad method to do it (maybe the worst) but I didn't find a better one yet!

            ThreadPool.QueueUserWorkItem(
                state =>
                {
                    var host = (DependencyObject)state;
                    var commandSource = (ICommandSource)state;

                    Func<bool> checkFunc = () =>
                    {
                        if (commandSource.Command != null)
                        {
                            OnCommandBindingResolved();
                            return true;
                        }
                        return false;
                    };

                    while (true)
                    {
                        if ((bool)host.Dispatcher.Invoke(checkFunc))
                            break;
                        Thread.Sleep(10);
                    }
                },
                AssociatedObject);
        }

        private void OnCommandBindingResolved()
        {
            var rootWindow = Window.GetWindow(AssociatedObject);
            if (rootWindow == null)
                return;

            var localCommand = ((ICommandSource)AssociatedObject).Command;
            var localKeyGesture = KeyGesture;

            if (localCommand == null || localKeyGesture == null)
                return;

            var binding = new KeyBinding(localCommand, localKeyGesture);

            bindingRegistration = new AnonymousDisposable(() => rootWindow.InputBindings.Remove(binding));
            rootWindow.InputBindings.Add(binding);
        }

        protected override void OnDetaching()
        {
            if (bindingRegistration != null)
            {
                bindingRegistration.Dispose();
                bindingRegistration = null;
            }
        }
    }
}
