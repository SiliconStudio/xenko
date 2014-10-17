// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Contains information about a tracked component.
    /// </summary>
    public class ComponentReference
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentReference"/> class.
        /// </summary>
        /// <param name="creationTime">The creation time.</param>
        /// <param name="component">The component to track.</param>
        public ComponentReference(DateTime creationTime, IComponent component)
        {
            CreationTime = creationTime;
            // Creates a short week reference to the Component
            Object = new WeakReference(component);
            Events = new List<ComponentEventInfo>();

            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {
                StackTrace = ex.StackTrace;
            }
        }

        /// <summary>
        /// Gets the stack trace when the track object was created.
        /// </summary>
        /// <value>The stack trace.</value>
        public string StackTrace { get; private set; }

        public List<ComponentEventInfo> Events { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the tracked object has been destroyed when tracking events.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is destroyed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDestroyed { get; internal set; }

        /// <summary>
        /// Gets the time the object was created.
        /// </summary>
        /// <value>The creation time.</value>
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// Gets a weak reference to the tracked object.
        /// </summary>
        /// <value>The weak reference to the tracked object.</value>
        public WeakReference Object { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the tracked object is alive.
        /// </summary>
        /// <value><c>true</c> if tracked object is alive; otherwise, <c>false</c>.</value>
        public bool IsAlive
        {
            get { return this.Object.IsAlive; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var component = this.Object.Target as IComponent;
            if (component == null)
                return string.Empty;

            var builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, "Active component Id: [{0}] Name: [{1}] Class: [{2}] Time [{3}] Stack: {4}", component.Id, component.Name, component.GetType().FullName, CreationTime, StackTrace).AppendLine();
            return builder.ToString();
        }
    }
}
