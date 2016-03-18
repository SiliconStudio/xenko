// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    /// <summary>
    /// Parameters used for compilation.
    /// </summary>
    [DataSerializer(typeof(DictionaryAllSerializer<CompilerParameters, ParameterKey, object>))]
    public sealed class CompilerParameters : ParameterCollection, IDictionary<ParameterKey, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerParameters"/> class.
        /// </summary>
        public CompilerParameters()
        {
        }

        public CompilerParameters(CompilerParameters compilerParameters) : base(compilerParameters)
        {
            EffectParameters = compilerParameters.EffectParameters;
        }

        [DataMemberIgnore]
        public EffectCompilerParameters EffectParameters = EffectCompilerParameters.Default;

        #region IDictionary<ParameterKey, object> implementation
        public void Add(ParameterKey key, object value)
        {
            SetObject(key, value);
        }

        public bool Contains(KeyValuePair<ParameterKey, object> item)
        {
            var accessor = GetObjectParameterHelper(item.Key);
            if (accessor.Offset == -1)
                return false;

            return ObjectValues[accessor.Offset].Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<ParameterKey, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                var count = 0;

                foreach (var parameterKeyInfo in ParameterKeyInfos)
                {
                    if (parameterKeyInfo.Key.Type == ParameterKeyType.Permutation)
                        count++;
                }

                return count;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<ParameterKey, object>> GetEnumerator()
        {
            foreach (var parameterKeyInfo in ParameterKeyInfos)
            {
                if (parameterKeyInfo.Key.Type != ParameterKeyType.Permutation)
                    continue;

                yield return new KeyValuePair<ParameterKey, object>(parameterKeyInfo.Key, ObjectValues[parameterKeyInfo.BindingSlot]);
            }
        }

        public bool IsReadOnly => false;

        public object this[ParameterKey key]
        {
            get { return GetObject(key); }
            set { SetObject(key, value); }
        }

        public ICollection<ParameterKey> Keys { get { throw new NotImplementedException(); } }
        public ICollection<object> Values { get { throw new NotImplementedException(); } }

        public bool TryGetValue(ParameterKey key, out object value)
        {
            foreach (var parameterKeyInfo in ParameterKeyInfos)
            {
                if (parameterKeyInfo.Key.Type != ParameterKeyType.Permutation)
                    continue;

                if (parameterKeyInfo.Key == key)
                {
                    value = ObjectValues[parameterKeyInfo.BindingSlot];
                    return true;
                }
            }

            value = null;
            return false;
        }

        public void Add(KeyValuePair<ParameterKey, object> item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<ParameterKey, object> item)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}