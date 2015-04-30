// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.Paradox.Rendering
{
    [Obsolete]
    internal class ParameterPath
    {
        public ParameterPath(params ParameterKey[] keys)
        {
            Keys = keys.ToArray();
        }

        public object GetValue(ParameterCollection parameterCollection)
        {
            for (int i = 0; i < Keys.Length; ++i)
            {
                if (!parameterCollection.ContainsKey(Keys[i]))
                    return null;

                var value = parameterCollection.GetObject(Keys[i]);
                
                // Last key, returns result
                if (i == Keys.Length - 1)
                    return value;
                
                // Ohterwise, it should be a container
                if (!(value is ParameterCollection))
                    return null;

                parameterCollection = (ParameterCollection)value;
            }

            return null;
        }

        internal ParameterKey[] Keys { get; set; }
    }

    [Obsolete]
    internal class ParameterListener
    {
        private List<KeyValuePair<ParameterKey, ParameterCollection>> containers = new List<KeyValuePair<ParameterKey, ParameterCollection>>();
        private List<ParameterCollection.ValueChangedDelegate> delegates = new List<ParameterCollection.ValueChangedDelegate>(); 

        private ParameterPath path;

        private object currentValue;

        public delegate void ParameterUpdatedDelegate(ParameterCollection container, ParameterPath path, object newValue);

        public event ParameterUpdatedDelegate ParameterUpdated;

        public ParameterListener(ParameterCollection parameterCollection, ParameterPath path)
        {
            this.path = path;

            foreach (var currentContainer in ContainersInPath(parameterCollection, path.Keys))
            {
                AppendCurrentPath(currentContainer);
            }

            currentValue = path.GetValue(containers[0].Value);
        }

        private void AppendCurrentPath(ParameterCollection parameterCollection)
        {
            ParameterCollection.ValueChangedDelegate currentDelegate;
            
            // Check if this container is already used at some point in the path (cyclic ref)
            var key = path.Keys[containers.Count];
            var keyAndContainer = new KeyValuePair<ParameterKey, ParameterCollection>(key, parameterCollection);
            var containerIndex = containers.IndexOf(keyAndContainer);

            if (containerIndex == -1)
            {
                var pathIndex = containers.Count;
                currentDelegate = (internalValueKey, internalValue, oldValue) =>
                    propertyContainer_PropertyUpdated(parameterCollection, internalValue.Object, oldValue, pathIndex);
                parameterCollection.AddEvent(key, currentDelegate);
            }
            else
            {
                currentDelegate = delegates[containerIndex];
            }

            containers.Add(keyAndContainer);
            delegates.Add(currentDelegate);
        }

        private static IEnumerable<ParameterCollection> ContainersInPath(ParameterCollection parameterCollection, IEnumerable<ParameterKey> keys)
        {
            yield return parameterCollection;
            foreach (var key in keys)
            {
                if (!parameterCollection.ContainsKey(key))
                    break;
                var nextContainer = parameterCollection.GetObject(key) as ParameterCollection;
                if (nextContainer == null)
                    break;
                yield return nextContainer;
                parameterCollection = nextContainer;
            }
        }
        
        private void propertyContainer_PropertyUpdated(ParameterCollection parameterCollection, object newValue, object oldValue, int pathIndex)
        {
            if (containers[pathIndex].Value != parameterCollection)
                throw new InvalidOperationException("Unexpected PropertyContainer in PathListener.");

            // Optimize case where last item only changed (no need to recreate subpath)
            if (pathIndex < path.Keys.Length - 1)
            {
                // Unregister listeners of this subpath
                for (int i = pathIndex + 1; i < containers.Count; ++i)
                {
                    // Only remove event handler if not present in first part of the path (otherwise we need to keep it)
                    if (containers.IndexOf(containers[i], 0, i) == -1)
                        containers[i].Value.RemoveEvent(path.Keys[i], delegates[i]);
                }

                // Remove containers of this subpath
                containers.RemoveRange(pathIndex + 1, containers.Count - pathIndex - 1);
                delegates.RemoveRange(pathIndex + 1, delegates.Count - pathIndex - 1);

                // Recreate subpath hierarchy
                foreach (var currentContainer in ContainersInPath(parameterCollection, path.Keys.Skip(pathIndex)).Skip(1))
                {
                    // Only add event handler if not already in the path (if same PropertyContainer is multiple time in the path)
                    AppendCurrentPath(currentContainer);
                }
            }

            var newValue2 = path.GetValue(containers[0].Value);
            if (ParameterUpdated != null && !ArePropertyValuesEqual(path.Keys.Last(), this.currentValue, newValue2))
                ParameterUpdated(containers[0].Value, path, newValue2);
            this.currentValue = newValue2;
        }

        private static bool ArePropertyValuesEqual(ParameterKey propertyKey, object propertyValue1, object propertyValue2)
        {
            var propertyType = propertyKey.PropertyType;

            if (!propertyType.GetTypeInfo().IsValueType && propertyType != typeof(string))
            {
                return object.ReferenceEquals(propertyValue1, propertyValue2);
            }

            return object.Equals(propertyValue1, propertyValue2);
        }
    }
}