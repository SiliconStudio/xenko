// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Allows to attach dynamic properties to an object at runtime. Note that in order to use this object at runtime, you need to set to <c>true</c> the <see cref="Enable"/> property.
    /// </summary>
    public class ShadowObject : Dictionary<ShadowObjectPropertyKey, object>
    {
        // Use a conditional weak table in order to attach properties and to 
        private static readonly ConditionalWeakTable<object, ShadowObject> Shadows = new ConditionalWeakTable<object, ShadowObject>();

        private Guid? id;
        private bool isIdentifiable;

        internal ShadowObject()
        {
        }

        private ShadowObject(Type type)
        {
            isIdentifiable = IdentifiableHelper.IsIdentifiable(type);
        }

        public bool IsIdentifiable => isIdentifiable;

        /// <summary>
        /// Gets or sets a boolean indicating whether this object is selected by an editor.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether this object is being mouse hovered from an editor.
        /// </summary>
        public bool IsHover { get; set; }

        /// <summary>
        /// Gets or sets a boolean to enable or disable shadow object. 
        /// </summary>
        /// <remarks>
        /// When disabled, method <see cref="Get"/> or <see cref="GetOrCreate"/>
        /// </remarks>
        public static bool Enable { get; set; }

        /// <summary>
        /// Checks if the following object instance is selected by an editor.
        /// </summary>
        /// <param name="instance">A live object instance</param>
        /// <returns><c>true</c> if the object is selected, false otherwise</returns>
        public static bool IsObjectSelected(object instance)
        {
            if (Enable)
            {
                var shadow = Get(instance);
                if (shadow != null)
                {
                    return shadow.IsSelected;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the following object instance is being mouse hovered from an editor.
        /// </summary>
        /// <param name="instance">A live object instance</param>
        /// <returns><c>true</c> if the object is selected, false otherwise</returns>
        public static bool IsObjectHover(object instance)
        {
            if (Enable)
            {
                var shadow = Get(instance);
                if (shadow != null)
                {
                    return shadow.IsHover;
                }
            }
            return false;
        }

        /// <summary>
        /// Tries to get the <see cref="ShadowObject"/> instance associated.
        /// </summary>
        /// <param name="instance">The live instance</param>
        /// <param name="shadow">The shadow object</param>
        /// <returns><c>true</c> if the shadow object was found, <c>false</c> otherwise</returns>
        public static bool TryGet(object instance, out ShadowObject shadow)
        {
            shadow = null;
            if (!Enable || instance == null) return false;
            return Shadows.TryGetValue(instance, out shadow);
        }

        /// <summary>
        /// Gets the <see cref="ShadowObject"/> instance if it exists or <c>null</c> otherwise.
        /// </summary>
        /// <param name="instance">The live instance.</param>
        /// <returns>The shadow instance or <c>null</c> if none</returns>
        public static ShadowObject Get(object instance)
        {
            if (!Enable || instance == null) return null;
            ShadowObject shadow;
            Shadows.TryGetValue(instance, out shadow);
            return shadow;
        }

        /// <summary>
        /// Gets the <see cref="ShadowObject"/> instance. Creates it if it does not exist.
        /// </summary>
        /// <param name="instance">The live instance.</param>
        /// <returns>The shadow instance</returns>
        public static ShadowObject GetOrCreate(object instance)
        {
            if (!Enable)
            {
                throw new InvalidOperationException("ShadowObject is not enabled. You need to enable it in order to use this method. Note also that ShadowObject has a performance cost at runtime");
            }

            if (instance == null) return null;
            var shadow = Shadows.GetValue(instance, callback => new ShadowObject(instance.GetType()));
            return shadow;
        }

        /// <summary>
        /// Copies all dynamic properties from an instance to another instance.
        /// </summary>
        /// <param name="fromInstance">The instance to copy the shadow attributes from</param>
        /// <param name="toInstance">The instance to copy the shadow attributes to</param>
        public static void Copy(object fromInstance, object toInstance)
        {
            if (!Enable) return;
            if (fromInstance == null) throw new ArgumentNullException(nameof(fromInstance));
            if (toInstance == null) throw new ArgumentNullException(nameof(toInstance));

            var type = fromInstance.GetType();

            // If the type is identifiable, we need to force the creation of a ShadowObject in order to
            // generate an id
            bool forceShadowCreation = IdentifiableHelper.IsIdentifiable(type);

            ShadowObject shadow;
            if (forceShadowCreation)
            {
                shadow = Shadows.GetValue(fromInstance, callback => new ShadowObject(fromInstance.GetType()));
            }
            else
            {
                Shadows.TryGetValue(fromInstance, out shadow);
            }

            if (shadow != null)
            {
                var newShadow = Shadows.GetValue(toInstance, key => new ShadowObject());
                shadow.CopyTo(newShadow);

                // Copy the id of the attached reference to the destination
                if (shadow.IsIdentifiable)
                {
                    newShadow.SetId(toInstance, shadow.GetId(fromInstance));
                }
            }
        }

        public Guid GetId(object instance)
        {
            // If the object is not identifiable, early exit
            if (!isIdentifiable)
            {
                return Guid.Empty;
            }

            // Don't use  local id if the object is already identifiable

            // If an object has an attached reference, we cannot use the id of the instance
            // So we need to use an auto-generated Id
            var attachedReference = AttachedReferenceManager.GetAttachedReference(instance);
            if (attachedReference == null)
            {
                var @component = instance as IIdentifiable;
                if (@component != null)
                {
                    return @component.Id;
                }
            }

            // If we don't have yet an id, create one.
            if (!id.HasValue)
            {
                id = Guid.NewGuid();
            }

            return id.Value;
        }

        public void SetId(object instance, Guid id)
        {
            // If the object is not identifiable, early exit
            if (!isIdentifiable)
            {
                return;
            }

            // If the object instance is already identifiable, store id into it directly
            var attachedReference = AttachedReferenceManager.GetAttachedReference(instance);
            var @component = instance as IIdentifiable;
            if (attachedReference == null && @component != null)
            {
                @component.Id = id;
            }
            else
            {
                this.id = id;
            }
        }

        public void CopyTo(ShadowObject copy)
        {
            copy.id = id;
            copy.isIdentifiable = isIdentifiable;
            copy.IsSelected = IsSelected;
            copy.IsHover = IsHover;

            foreach (var keyValue in this)
            {
                copy.Add(keyValue.Key, keyValue.Value);
            }
        }
    }
}