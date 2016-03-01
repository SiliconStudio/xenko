// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Reflection;

namespace SiliconStudio.Presentation.Core
{
    public class AnonymousEventHandler
    {
        protected Action Action;
        private Delegate eventHandler;
        private EventInfo eventInfo;
        private object target;

        public static AnonymousEventHandler RegisterEventHandler(EventInfo eventInfo, object target, Action handler)
        {
            ParameterInfo[] parameterInfos = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters();

            if (parameterInfos.Length != 2)
                throw new ArgumentException("The given event info must have exactly two parameters.");

            Type argumentType = parameterInfos.Skip(1).First().ParameterType;
            Type type = typeof(AnonymousEventHandler<>).MakeGenericType(argumentType);

            MethodInfo method = type.GetMethod("Handler");
            var anonymousHandler = (AnonymousEventHandler)Activator.CreateInstance(type);
            anonymousHandler.Action = handler;
            anonymousHandler.eventHandler = Delegate.CreateDelegate(eventInfo.EventHandlerType, anonymousHandler, method);
            anonymousHandler.eventInfo = eventInfo;
            anonymousHandler.target = target;
            eventInfo.AddEventHandler(target, anonymousHandler.eventHandler);

            return anonymousHandler;
        }

        public static void UnregisterEventHandler(AnonymousEventHandler handler)
        {
            handler.eventInfo.RemoveEventHandler(handler.target, handler.eventHandler);
        }
    }

    /// <summary>
    /// This class allow to register an anonymous handler to an event using reflection.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of <see cref="EventArgs"/> expected by the event.</typeparam>
    /// <seealso cref="AnonymousEventHandler.RegisterEventHandler"/>
    internal class AnonymousEventHandler<TEventArgs> : AnonymousEventHandler where TEventArgs : EventArgs
    {
        public void Handler(object sender, TEventArgs e)
        {
            Action();
        }
    } 
}
