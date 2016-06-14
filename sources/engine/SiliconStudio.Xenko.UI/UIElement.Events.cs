// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.UI.Events;

namespace SiliconStudio.Xenko.UI
{
    public abstract partial class UIElement : IUIElementUpdate
    {
        #region Routed Events

        private static readonly RoutedEvent<TouchEventArgs> previewTouchDownEvent = EventManager.RegisterRoutedEvent<TouchEventArgs>(
            "PreviewTouchDown",
            RoutingStrategy.Tunnel,
            typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> previewTouchMoveEvent = EventManager.RegisterRoutedEvent<TouchEventArgs>(
            "PreviewTouchMove",
            RoutingStrategy.Tunnel,
            typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> previewTouchUpEvent = EventManager.RegisterRoutedEvent<TouchEventArgs>(
            "PreviewTouchUp",
            RoutingStrategy.Tunnel,
            typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> touchDownEvent = EventManager.RegisterRoutedEvent<TouchEventArgs>(
            "TouchDown",
            RoutingStrategy.Bubble,
            typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> touchEnterEvent = EventManager.RegisterRoutedEvent<TouchEventArgs>(
            "TouchEnter",
            RoutingStrategy.Direct,
            typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> touchLeaveEvent = EventManager.RegisterRoutedEvent<TouchEventArgs>(
            "TouchLeave",
            RoutingStrategy.Direct,
            typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> touchMoveEvent = EventManager.RegisterRoutedEvent<TouchEventArgs>(
            "TouchMove",
            RoutingStrategy.Bubble,
            typeof(UIElement));

        private static readonly RoutedEvent<TouchEventArgs> touchUpEvent = EventManager.RegisterRoutedEvent<TouchEventArgs>(
            "TouchUp",
            RoutingStrategy.Bubble,
            typeof(UIElement));

        private static readonly RoutedEvent<KeyEventArgs> keyPressedEvent = EventManager.RegisterRoutedEvent<KeyEventArgs>(
            "KeyPressed",
            RoutingStrategy.Bubble,
            typeof(UIElement));

        private static readonly RoutedEvent<KeyEventArgs> keyDownEvent = EventManager.RegisterRoutedEvent<KeyEventArgs>(
            "KeyDown",
            RoutingStrategy.Bubble,
            typeof(UIElement));

        private static readonly RoutedEvent<KeyEventArgs> keyReleasedEvent = EventManager.RegisterRoutedEvent<KeyEventArgs>(
            "KeyReleased",
            RoutingStrategy.Bubble,
            typeof(UIElement));

        #endregion

        private readonly static Queue<List<RoutedEventHandlerInfo>> routedEventHandlerInfoListPool = new Queue<List<RoutedEventHandlerInfo>>();

        static UIElement()
        {
            // register the class handlers
            EventManager.RegisterClassHandler(typeof(UIElement), previewTouchDownEvent, PreviewTouchDownClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), previewTouchMoveEvent, PreviewTouchMoveClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), previewTouchUpEvent, PreviewTouchUpClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), touchDownEvent, TouchDownClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), touchEnterEvent, TouchEnterClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), touchLeaveEvent, TouchLeaveClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), touchMoveEvent, TouchMoveClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), touchUpEvent, TouchUpClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), keyPressedEvent, KeyPressedClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), keyDownEvent, KeyDownClassHandler);
            EventManager.RegisterClassHandler(typeof(UIElement), keyReleasedEvent, KeyReleasedClassHandler);
        }
    

        /// <summary>
        /// Gets a value indicating whether the <see cref="UIElement"/> is currently touched by the user.
        /// </summary>
        public bool IsTouched { get; internal set; }
       

        /// <summary>
        /// Gets the current state of the mouse over the UI element.
        /// </summary>
        /// <remarks>Only elements that can be clicked by user can have the <cref>MouseOverState.MouseOverElement</cref> value. 
        /// That is element that have <see cref="CanBeHitByUser"/> set to <value>true</value></remarks>
        public MouseOverState MouseOverState
        {
            get { return mouseOverState; }
            internal set
            {
                var oldValue = mouseOverState;
                if (oldValue == value)
                    return;

                mouseOverState = value;

                MouseOverStateChanged?.Invoke(this, new PropertyChangedArgs<MouseOverState> { NewValue = value, OldValue = oldValue });
            }
        }
      
        internal void PropagateRoutedEvent(RoutedEventArgs e)
        {
            var routedEvent = e.RoutedEvent;

            // propagate first if tunneling
            if (routedEvent.RoutingStrategy == RoutingStrategy.Tunnel)
                VisualParent?.PropagateRoutedEvent(e);

            // Trigger the class handler
            var classHandler = EventManager.GetClassHandler(GetType(), routedEvent);
            if (classHandler != null && (classHandler.HandledEventToo || !e.Handled))
                classHandler.Invoke(this, e);

            // Trigger instance handlers
            if (eventsToHandlers.ContainsKey(routedEvent))
            {
                // get a list of handler from the pool where we can copy the handler to trigger
                if (routedEventHandlerInfoListPool.Count == 0)
                    routedEventHandlerInfoListPool.Enqueue(new List<RoutedEventHandlerInfo>());
                var pooledList = routedEventHandlerInfoListPool.Dequeue();

                // copy the RoutedEventHandlerEventInfo list into a list of the pool in order to be able to modify the handler list in the handler itself
                pooledList.AddRange(eventsToHandlers[routedEvent]);

                // iterate on the pooled list to invoke handlers
                foreach (var handlerInfo in pooledList)
                {
                    if (handlerInfo.HandledEventToo || !e.Handled)
                        handlerInfo.Invoke(this, e);
                }

                // add the pooled list back to the pool.
                pooledList.Clear(); // avoid to keep dead references
                routedEventHandlerInfoListPool.Enqueue(pooledList);
            }

            // propagate afterwards if bubbling
            if (routedEvent.RoutingStrategy == RoutingStrategy.Bubble)
                VisualParent?.PropagateRoutedEvent(e);
        }

        /// <summary>
        /// Raises a specific routed event. The <see cref="RoutedEvent"/> to be raised is identified within the <see cref="RoutedEventArgs"/> instance 
        /// that is provided (as the <see cref="RoutedEvent"/> property of that event data).
        /// </summary>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data and also identifies the event to raise.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The type of the routed event argument <paramref name="e"/> does not match the event handler second argument type.</exception>
        public void RaiseEvent(RoutedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (e.RoutedEvent == null)
                return;

            if (!e.RoutedEvent.HandlerSecondArgumentType.GetTypeInfo().IsAssignableFrom(e.GetType().GetTypeInfo()))
                throw new InvalidOperationException("The type of second parameter of the handler (" + e.RoutedEvent.HandlerSecondArgumentType
                                                        + ") is not assignable from the parameter 'e' type (" + e.GetType() + ").");

            var sourceWasNull = e.Source == null;
            if (sourceWasNull)  // set the source to default if needed
                e.Source = this;

            e.StartEventRouting();

            PropagateRoutedEvent(e);

            e.EndEventRouting();

            if (sourceWasNull) // reset the source if it was not explicitly set (event might be reused again for other sources)
                e.Source = null;
        }

        /// <summary>
        /// Adds a routed event handler for a specified routed event, adding the handler to the handler collection on the current element. 
        /// Specify handledEventsToo as true to have the provided handler be invoked for routed event that had already been marked as handled by another element along the event route.
        /// </summary>
        /// <param name="routedEvent">An identifier for the routed event to be handled.</param>
        /// <param name="handler">A reference to the handler implementation.</param>
        /// <param name="handledEventsToo">true to register the handler such that it is invoked even when the routed event is marked handled in its event data; 
        /// false to register the handler with the default condition that it will not be invoked if the routed event is already marked handled.</param>
        /// <exception cref="ArgumentNullException">Provided handler or routed event is null.</exception>
        public void AddHandler<T>(RoutedEvent<T> routedEvent, EventHandler<T> handler, bool handledEventsToo = false) where T : RoutedEventArgs
        {
            if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!eventsToHandlers.ContainsKey(routedEvent))
                eventsToHandlers[routedEvent] = new List<RoutedEventHandlerInfo>();

            eventsToHandlers[routedEvent].Add(new RoutedEventHandlerInfo<T>(handler, handledEventsToo));
        }

        /// <summary>
        /// Removes the specified routed event handler from this element.
        /// </summary>
        /// <param name="routedEvent">The identifier of the routed event for which the handler is attached.</param>
        /// <param name="handler">The specific handler implementation to remove from the event handler collection on this element.</param>
        /// <exception cref="ArgumentNullException">Provided handler or routed event is null.</exception>
        public void RemoveHandler<T>(RoutedEvent<T> routedEvent, EventHandler<T> handler) where T : RoutedEventArgs
        {
            if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!eventsToHandlers.ContainsKey(routedEvent))
                return;

            eventsToHandlers[routedEvent].Remove(new RoutedEventHandlerInfo<T>(handler));
        }

        private readonly Dictionary<RoutedEvent, List<RoutedEventHandlerInfo>> eventsToHandlers = new Dictionary<RoutedEvent, List<RoutedEventHandlerInfo>>();

        #region Events

        /// <summary>
        /// Occurs when the value of the <see cref="MouseOverState"/> property changed.
        /// </summary>
        /// <remarks>This event is not a routed event</remarks>
        public event PropertyChangedHandler<MouseOverState> MouseOverStateChanged;

        /// <summary>
        /// Occurs when the user starts touching the <see cref="UIElement"/>. That is when he moves its finger down from the element.
        /// </summary>
        /// <remarks>A click event is tunneling</remarks>
        public event EventHandler<TouchEventArgs> PreviewTouchDown
        {
            add { AddHandler(previewTouchDownEvent, value); }
            remove { RemoveHandler(previewTouchDownEvent, value); }
        }

        /// <summary>
        /// Occurs when the user moves its finger on the <see cref="UIElement"/>. 
        /// That is when his finger was already on the element and moved from its previous position.
        /// </summary>
        /// <remarks>A click event is tunneling</remarks>
        public event EventHandler<TouchEventArgs> PreviewTouchMove
        {
            add { AddHandler(previewTouchMoveEvent, value); }
            remove { RemoveHandler(previewTouchMoveEvent, value); }
        }

        /// <summary>
        /// Occurs when the user stops touching the <see cref="UIElement"/>. That is when he moves its finger up from the element.
        /// </summary>
        /// <remarks>A click event is tunneling</remarks>
        public event EventHandler<TouchEventArgs> PreviewTouchUp
        {
            add { AddHandler(previewTouchUpEvent, value); }
            remove { RemoveHandler(previewTouchUpEvent, value); }
        }

        /// <summary>
        /// Occurs when the user starts touching the <see cref="UIElement"/>. That is when he moves its finger down from the element.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<TouchEventArgs> TouchDown
        {
            add { AddHandler(touchDownEvent, value); }
            remove { RemoveHandler(touchDownEvent, value); }
        }

        /// <summary>
        /// Occurs when the user enters its finger into <see cref="UIElement"/>. 
        /// That is when his finger was on the screen outside of the element and moved inside the element.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<TouchEventArgs> TouchEnter
        {
            add { AddHandler(touchEnterEvent, value); }
            remove { RemoveHandler(touchEnterEvent, value); }
        }

        /// <summary>
        /// Occurs when the user leaves its finger from the <see cref="UIElement"/>. 
        /// That is when his finger was inside of the element and moved on the screen outside of the element.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<TouchEventArgs> TouchLeave
        {
            add { AddHandler(touchLeaveEvent, value); }
            remove { RemoveHandler(touchLeaveEvent, value); }
        }

        /// <summary>
        /// Occurs when the user move its finger inside the <see cref="UIElement"/>.
        /// That is when his finger was already on the element and moved from its previous position.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<TouchEventArgs> TouchMove
        {
            add { AddHandler(touchMoveEvent, value); }
            remove { RemoveHandler(touchMoveEvent, value); }
        }

        /// <summary>
        /// Occurs when the user stops touching the <see cref="UIElement"/>. That is when he moves its finger up from the element.
        /// </summary>
        /// <remarks>A click event is bubbling</remarks>
        public event EventHandler<TouchEventArgs> TouchUp
        {
            add { AddHandler(touchUpEvent, value); }
            remove { RemoveHandler(touchUpEvent, value); }
        }

        /// <summary>
        /// Occurs when the element has the focus and the user press a key on the keyboard.
        /// </summary>
        /// <remarks>A key pressed event is bubbling</remarks>
        internal event EventHandler<KeyEventArgs> KeyPressed
        {
            add { AddHandler(keyPressedEvent, value); }
            remove { RemoveHandler(keyPressedEvent, value); }
        }

        /// <summary>
        /// Occurs when the element has the focus and the user maintains a key pressed on the keyboard.
        /// </summary>
        /// <remarks>A key down event is bubbling</remarks>
        internal event EventHandler<KeyEventArgs> KeyDown
        {
            add { AddHandler(keyDownEvent, value); }
            remove { RemoveHandler(keyDownEvent, value); }
        }

        /// <summary>
        /// Occurs when the element has the focus and the user release a key on the keyboard.
        /// </summary>
        /// <remarks>A key released event is bubbling</remarks>
        internal event EventHandler<KeyEventArgs> KeyReleased
        {
            add { AddHandler(keyReleasedEvent, value); }
            remove { RemoveHandler(keyReleasedEvent, value); }
        }

        #endregion

        #region Internal Event Raiser

        internal void RaiseTouchDownEvent(TouchEventArgs touchArgs)
        {
            touchArgs.RoutedEvent = previewTouchDownEvent;
            RaiseEvent(touchArgs);

            touchArgs.RoutedEvent = touchDownEvent;
            RaiseEvent(touchArgs);
        }

        internal void RaiseTouchEnterEvent(TouchEventArgs touchArgs)
        {
            touchArgs.RoutedEvent = touchEnterEvent;
            RaiseEvent(touchArgs);
        }

        internal void RaiseTouchLeaveEvent(TouchEventArgs touchArgs)
        {
            touchArgs.RoutedEvent = touchLeaveEvent;
            RaiseEvent(touchArgs);
        }

        internal void RaiseTouchMoveEvent(TouchEventArgs touchArgs)
        {
            touchArgs.RoutedEvent = previewTouchMoveEvent;
            RaiseEvent(touchArgs);

            touchArgs.RoutedEvent = touchMoveEvent;
            RaiseEvent(touchArgs);
        }

        internal void RaiseTouchUpEvent(TouchEventArgs touchArgs)
        {
            touchArgs.RoutedEvent = previewTouchUpEvent;
            RaiseEvent(touchArgs);

            touchArgs.RoutedEvent = touchUpEvent;
            RaiseEvent(touchArgs);
        }

        internal void RaiseKeyPressedEvent(KeyEventArgs keyEventArgs)
        {
            keyEventArgs.RoutedEvent = keyPressedEvent;
            RaiseEvent(keyEventArgs);
        }

        internal void RaiseKeyDownEvent(KeyEventArgs keyEventArgs)
        {
            keyEventArgs.RoutedEvent = keyDownEvent;
            RaiseEvent(keyEventArgs);
        }

        internal void RaiseKeyReleasedEvent(KeyEventArgs keyEventArgs)
        {
            keyEventArgs.RoutedEvent = keyReleasedEvent;
            RaiseEvent(keyEventArgs);
        }

        #endregion

        #region Class Event Handlers

        private static void PreviewTouchDownClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnPreviewTouchDown(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="PreviewTouchDown"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnPreviewTouchDown(TouchEventArgs args)
        {
            IsTouched = true;
        }

        private static void PreviewTouchMoveClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnPreviewTouchMove(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="PreviewTouchMove"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnPreviewTouchMove(TouchEventArgs args)
        {
        }

        private static void PreviewTouchUpClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnPreviewTouchUp(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="PreviewTouchUp"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnPreviewTouchUp(TouchEventArgs args)
        {
            IsTouched = false;
        }

        private static void TouchDownClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnTouchDown(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="TouchDown"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnTouchDown(TouchEventArgs args)
        {
        }

        private static void TouchEnterClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnTouchEnter(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="TouchEnter"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnTouchEnter(TouchEventArgs args)
        {
            IsTouched = true;
        }

        private static void TouchLeaveClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnTouchLeave(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="TouchLeave"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnTouchLeave(TouchEventArgs args)
        {
            IsTouched = false;
        }

        private static void TouchMoveClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnTouchMove(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="TouchMove"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnTouchMove(TouchEventArgs args)
        {
        }

        private static void TouchUpClassHandler(object sender, TouchEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnTouchUp(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="TouchUp"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        protected virtual void OnTouchUp(TouchEventArgs args)
        {
        }

        private static void KeyPressedClassHandler(object sender, KeyEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnKeyPressed(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="KeyPressed"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        internal virtual void OnKeyPressed(KeyEventArgs args)
        {
        }

        private static void KeyDownClassHandler(object sender, KeyEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnKeyDown(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="KeyDown"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        internal virtual void OnKeyDown(KeyEventArgs args)
        {
        }

        private static void KeyReleasedClassHandler(object sender, KeyEventArgs args)
        {
            var uiElementSender = (UIElement)sender;
            if (uiElementSender.IsHierarchyEnabled)
                uiElementSender.OnKeyReleased(args);
        }

        /// <summary>
        /// The class handler of the event <see cref="KeyReleased"/>.
        /// This method can be overridden in inherited classes to perform actions common to all instances of a class.
        /// </summary>
        /// <param name="args">The arguments of the event</param>
        internal virtual void OnKeyReleased(KeyEventArgs args)
        {
        }

        #endregion
    }
}
