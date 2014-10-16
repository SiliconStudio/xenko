// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Inheritance only applies to specific keys (which can be remapped).
    /// </summary>
    public class InheritanceDefinition : IParameterCollectionInheritanceInternal, System.Collections.IEnumerable
    {
        private ParameterCollection source;
        private Dictionary<ParameterKey, ParameterKey> keyMapping;
        private ParameterCollection.OnUpdateValueDelegate updateValueDelegate;

        public InheritanceDefinition(ParameterCollection source)
        {
            this.source = source;
            this.keyMapping = new Dictionary<ParameterKey, ParameterKey>();
        }

        public void Add(ParameterKey keySource, ParameterKey keyTarget)
        {
            keyMapping.Add(keySource, keyTarget);
        }

        public void Add(ParameterKey key)
        {
            keyMapping.Add(key, key);
        }

        ParameterKey MapKey(ParameterKey key)
        {
            ParameterKey mappedKey;
            if (keyMapping.TryGetValue(key, out mappedKey))
                return mappedKey;

            return key;
        }

        ParameterCollection IParameterCollectionInheritanceInternal.GetParameterCollection()
        {
            return source;
        }

        ParameterCollection.OnUpdateValueDelegate IParameterCollectionInheritanceInternal.GetUpdateValueDelegate(ParameterCollection.OnUpdateValueDelegate original)
        {
            if (updateValueDelegate == null)
            {
                updateValueDelegate = (source, key, value) =>
                    {
                        if (keyMapping.ContainsKey(key))
                        {
                            original(source, keyMapping[key], value);
                        }
                    };
            }
            return updateValueDelegate;
        }

        int IParameterCollectionInheritanceInternal.GetInternalValueCount()
        {
            return keyMapping.Count;
        }

        ParameterCollection.InternalValue IParameterCollectionInheritanceInternal.GetInternalValue(ParameterKey key)
        {
            return source.GetInternalValue(keyMapping[key]);
        }

        IEnumerable<KeyValuePair<ParameterKey, ParameterCollection.InternalValue>> IParameterCollectionInheritanceInternal.GetInternalValues()
        {
            return source.InternalValues.Where(x => x.Value != null && keyMapping.ContainsKey(x.Key)).Select(x => new KeyValuePair<ParameterKey, ParameterCollection.InternalValue>(keyMapping[x.Key], x.Value));
        }

        public IEnumerable<ParameterDynamicValue> DynamicValues
        {
            get
            {
                foreach (var dynamicValue in source.DynamicValues)
                {
                    var dynamicValueCopy = dynamicValue.Clone();
                    dynamicValueCopy.Dependencies = dynamicValue.Dependencies.Select(MapKey).ToArray();
                    dynamicValueCopy.Target = MapKey(dynamicValue.Target);
                    yield return dynamicValueCopy;
                }
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}