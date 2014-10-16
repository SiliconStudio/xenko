// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Applies IndexedOf(index) on each key.
    /// </summary>
    public class InheritanceIndexer : IParameterCollectionInheritanceInternal
    {
        private ParameterCollection source;
        private int keyIndex;
        private Dictionary<ParameterKey, ParameterKey> keyMapping = new Dictionary<ParameterKey, ParameterKey>();
        private ParameterCollection.OnUpdateValueDelegate updateValueDelegate;

        public InheritanceIndexer(ParameterCollection source, int keyIndex)
        {
            this.source = source;
            this.keyIndex = keyIndex;
        }

        ParameterCollection IParameterCollectionInheritanceInternal.GetParameterCollection()
        {
            return source;
        }

        ParameterKey MapKey(ParameterKey key, bool createIfMissing = true)
        {
            ParameterKey mappedKey;
            bool keyFound = keyMapping.TryGetValue(key, out mappedKey);

            if (keyFound)
                return mappedKey;

            if (!createIfMissing)
                return key;

            keyMapping.Add(key, mappedKey = key.AppendKey(keyIndex));
            return mappedKey;
        }

        ParameterCollection.OnUpdateValueDelegate IParameterCollectionInheritanceInternal.GetUpdateValueDelegate(ParameterCollection.OnUpdateValueDelegate original)
        {
            if (updateValueDelegate == null)
            {
                updateValueDelegate = (source, key, value) =>
                                          {
                                              original(source, MapKey(key), value);
                                          };
            }
            return updateValueDelegate;
        }

        int IParameterCollectionInheritanceInternal.GetInternalValueCount()
        {
            return ((IParameterCollectionInheritanceInternal)source).GetInternalValueCount();
        }


        ParameterCollection.InternalValue IParameterCollectionInheritanceInternal.GetInternalValue(ParameterKey key)
        {
            return source.GetInternalValue(MapKey(key));
        }

        IEnumerable<KeyValuePair<ParameterKey, ParameterCollection.InternalValue>> IParameterCollectionInheritanceInternal.GetInternalValues()
        {
            return source.InternalValues.Where(x => x.Value != null).Select(x => new KeyValuePair<ParameterKey, ParameterCollection.InternalValue>(MapKey(x.Key), x.Value));
        }

        public IEnumerable<ParameterDynamicValue> DynamicValues
        {
            get
            {
                foreach (var dynamicValue in source.DynamicValues)
                {
                    var dynamicValueCopy = dynamicValue.Clone();

                    // Remap keys (only if existing)
                    dynamicValueCopy.Dependencies = dynamicValue.Dependencies.Select(x => MapKey(x, false)).ToArray();
                    dynamicValueCopy.Target = MapKey(dynamicValue.Target);

                    yield return dynamicValueCopy;
                }
            }
        }
    }
}