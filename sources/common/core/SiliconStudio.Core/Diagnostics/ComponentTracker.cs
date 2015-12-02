// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Track all allocated objects.
    /// </summary>
    public class ComponentTracker
    {
        /// <summary>
        /// Enable ComponentTracker.
        /// </summary>
        public static readonly bool Enable = false;

        /// <summary>
        /// Enable ComponentTracker event tracking system.
        /// </summary>
        public static readonly bool EnableEvents = false;

        private static readonly Dictionary<Guid, ComponentReference> ObjectReferences = new Dictionary<Guid, ComponentReference>();
      
        /// <summary>
        /// Tracks the specified component object.
        /// </summary>
        /// <param name="component">The component object.</param>
        public static void Track(IComponent component)
        {
            if (component == null)
                return;
            lock (ObjectReferences)
            {
                ComponentReference componentReference;

                // Object is already tracked
                if (!ObjectReferences.TryGetValue(component.Id, out componentReference))
                {
                    ObjectReferences.Add(component.Id, new ComponentReference(DateTime.Now, component));
                }
            }
            if (Enable && EnableEvents)
                NotifyEvent(component, ComponentEventType.Instantiate);
        }

        /// <summary>
        /// Finds a component reference from a specified id.
        /// </summary>
        /// <param name="id">The id of the component</param>
        /// <returns>A component reference</returns>
        public static ComponentReference Find(Guid id)
        {
            lock (ObjectReferences)
            {
                ComponentReference componentReference;

                // Object is already tracked
                if (ObjectReferences.TryGetValue(id, out componentReference))
                    return componentReference;
            }
            return null;
        }

        /// <summary>
        /// Finds a component reference for a specific component.
        /// </summary>
        /// <param name="component">The component instance.</param>
        /// <returns>A component reference</returns>
        public static ComponentReference Find(IComponent component)
        {
            return Find(component.Id);
        }

        /// <summary>
        /// Untracks the specified component.
        /// </summary>
        /// <param name="component">The COM object.</param>
        public static void UnTrack(IComponent component)
        {
            if (component == null)
                return;

            if (Enable && EnableEvents)
            {
                NotifyEvent(component, ComponentEventType.Destroy);
            }
            else
            {
                // Only remove it if we don't track events.
                lock (ObjectReferences)
                {
                    ObjectReferences.Remove(component.Id);
                }
            }
        }

        /// <summary>
        /// Should be called everytime an event happens for a IComponent.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="eventType">Type of the event.</param>
        public static void NotifyEvent(IComponent component, ComponentEventType eventType)
        {
            ComponentReference componentReference;
            
            // Object is already tracked
            if (ObjectReferences.TryGetValue(component.Id, out componentReference))
            {
                componentReference.Events.Add(new ComponentEventInfo(eventType));
                if (eventType == ComponentEventType.Destroy)
                {
                    componentReference.IsDestroyed = true;
                }
            }
        }

        /// <summary>
        /// Reports all COM and IReferencable object that are active and not yet disposed.
        /// </summary>
        /// <returns>The list of active objects.</returns>
        public static List<ComponentReference> FindActiveObjects()
        {
            var activeObjects = new List<ComponentReference>();
            lock (ObjectReferences)
            {
                activeObjects.AddRange(ObjectReferences.Values.Where(x => !x.IsDestroyed));
            }

            return activeObjects;
        }

        /// <summary>
        /// Reports all COM object that are active and not yet disposed.
        /// </summary>
        /// <returns>The report about active objects.</returns>
        public static string ReportActiveObjects()
        {
            var text = new StringBuilder();
            foreach (var findActiveObject in FindActiveObjects())
            {
                var findActiveObjectStr = findActiveObject.ToString();
                if (!string.IsNullOrEmpty(findActiveObjectStr))
                    text.AppendLine(findActiveObjectStr);
            }
            return text.ToString();
        }
    }
}
