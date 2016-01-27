// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;

namespace SiliconStudio.Presentation.Core
{
    public class AnonymousEventHandler
    {
        private Action<EventArgs> action;
        private Delegate eventHandler;
        private EventInfo eventInfo;
        private object target;

        public static AnonymousEventHandler RegisterEventHandler(EventInfo eventInfo, object target, Action<EventArgs> handler)
        {
            var parameterInfos = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters();

            if (parameterInfos.Length != 2)
                throw new ArgumentException("The given event info must have exactly two parameters.");
            
            var type = typeof(AnonymousEventHandler);

            var method = type.GetMethod(nameof(AnonymousEventHandler.Handler));
            var anonymousHandler = new AnonymousEventHandler
            {
                action = handler,
                eventInfo = eventInfo,
                target = target,
            };
            anonymousHandler.eventHandler = Delegate.CreateDelegate(eventInfo.EventHandlerType, anonymousHandler, method);
            eventInfo.AddEventHandler(target, anonymousHandler.eventHandler);

            return anonymousHandler;
        }

        public static void UnregisterEventHandler(AnonymousEventHandler handler)
        {
            handler.eventInfo.RemoveEventHandler(handler.target, handler.eventHandler);
        }

        public void Handler(object sender, EventArgs e)
        {
            action(e);
        }
    }
}
