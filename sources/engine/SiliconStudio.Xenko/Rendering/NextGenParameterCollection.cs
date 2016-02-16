using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Manage several effect parameters (resources and data). A specific data and resource layout can be forced (usually by the consuming effect).
    /// </summary>
    [DataSerializer(typeof(NextGenParameterCollection.Serializer))]
    [DataSerializerGlobal(null, typeof(FastList<ParameterKeyInfo>))]
    public class NextGenParameterCollection : IDisposable
    {
        private FastListStruct<ParameterKeyInfo> layoutParameterKeyInfos;

        // TODO: Switch to FastListStruct (for serialization)
        private FastList<ParameterKeyInfo> parameterKeyInfos = new FastList<ParameterKeyInfo>(4);

        // Constants and resources
        // TODO: Currently stored in unmanaged array so we can get a pointer that can be updated from outside
        //   However, maybe ref locals would make this not needed anymore?
        public IntPtr DataValues;
        public int DataValuesSize;
        public object[] ObjectValues;

        public int PermutationCounter;

        public IEnumerable<ParameterKeyInfo> ParameterKeyInfos => parameterKeyInfos;

        public bool HasLayout => layoutParameterKeyInfos.Items != null;

        public void Dispose()
        {
            if (DataValues != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(DataValues);
                DataValues = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Gets an accessor to get and set objects more quickly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterKey"></param>
        /// <returns></returns>
        public ObjectParameterAccessor<T> GetAccessor<T>(ObjectParameterKey<T> parameterKey)
        {
            return GetObjectParameterHelper(parameterKey, false);
        }

        /// <summary>
        /// Gets an accessor to get and set permutations more quickly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterKey"></param>
        /// <returns></returns>
        public PermutationParameter<T> GetAccessor<T>(PermutationParameterKey<T> parameterKey)
        {
            // Remap it as PermutationParameter
            return new PermutationParameter<T>(GetObjectParameterHelper(parameterKey, true).Index);
        }

        /// <summary>
        /// Gets an accessor to get and set blittable values more quickly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterKey"></param>
        /// <returns></returns>
        public ValueParameter<T> GetAccessor<T>(ValueParameterKey<T> parameterKey, int elementCount = 1) where T : struct
        {
            // Find existing first
            for (int i = 0; i < parameterKeyInfos.Count; ++i)
            {
                if (parameterKeyInfos[i].Key == parameterKey)
                {
                    return new ValueParameter<T>(i);
                }
            }

            // Check layout if it exists
            if (layoutParameterKeyInfos.Count > 0)
            {
                foreach (var layoutParameterKeyInfo in layoutParameterKeyInfos)
                {
                    if (layoutParameterKeyInfo.Key == parameterKey)
                    {
                        parameterKeyInfos.Add(layoutParameterKeyInfo);
                        return new ValueParameter<T>(parameterKeyInfos.Count - 1);
                    }
                }
            }

            // Compute size
            var elementSize = parameterKey.Size;
            var totalSize = elementSize;
            if (elementCount > 1)
                totalSize += (elementSize + 15) / 16 * 16 * (elementCount - 1);

            // Create offset entry
            var result = new ValueParameter<T>(parameterKeyInfos.Count);
            var memberOffset = DataValuesSize;
            parameterKeyInfos.Add(new ParameterKeyInfo(parameterKey, memberOffset, totalSize));

            // We append at the end; resize array to accomodate new data
            DataValuesSize += totalSize;
            DataValues = DataValues != IntPtr.Zero
                ? Marshal.ReAllocHGlobal(DataValues, (IntPtr)DataValuesSize)
                : Marshal.AllocHGlobal((IntPtr)DataValuesSize);

            // Initialize default value
            if (parameterKey.DefaultValueMetadata != null)
            {
                parameterKey.DefaultValueMetadata.WriteBuffer(DataValues + memberOffset, 16);
            }

            return result;
        }

        [Obsolete]
        public ValueParameter<T> GetValueParameterArray<T>(ValueParameterKey<T> parameterKey, int elementCount = 1) where T : struct
        {
            // Find existing first
            for (int i = 0; i < parameterKeyInfos.Count; ++i)
            {
                if (parameterKeyInfos[i].Key == parameterKey)
                {
                    return new ValueParameter<T>(i);
                }
            }

            // Check layout if it exists
            if (layoutParameterKeyInfos.Count > 0)
            {
                foreach (var layoutParameterKeyInfo in layoutParameterKeyInfos)
                {
                    if (layoutParameterKeyInfo.Key == parameterKey)
                    {
                        parameterKeyInfos.Add(layoutParameterKeyInfo);
                        return new ValueParameter<T>(parameterKeyInfos.Count - 1);
                    }
                }
            }

            // Compute size
            var elementSize = parameterKey.Size;
            var totalSize = elementSize;
            if (elementCount > 1)
                totalSize += (elementSize + 15) / 16 * 16 * (elementCount - 1);

            // Create offset entry
            var result = new ValueParameter<T>(parameterKeyInfos.Count);
            var memberOffset = DataValuesSize;
            parameterKeyInfos.Add(new ParameterKeyInfo(parameterKey, memberOffset, totalSize));

            // We append at the end; resize array to accomodate new data
            DataValuesSize += totalSize;
            DataValues = DataValues != IntPtr.Zero
                ? Marshal.ReAllocHGlobal(DataValues, (IntPtr)DataValuesSize)
                : Marshal.AllocHGlobal((IntPtr)DataValuesSize);

            // Initialize default value
            if (parameterKey.DefaultValueMetadata != null)
            {
                parameterKey.DefaultValueMetadata.WriteBuffer(DataValues + memberOffset, 16);
            }

            return result;
        }

        /// <summary>
        /// Gets pointer to directly copy blittable values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public IntPtr GetValuePointer<T>(ValueParameter<T> parameter) where T : struct
        {
            return DataValues + parameterKeyInfos[parameter.Index].Offset;
        }

        /// <summary>
        /// Sets an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public void Set<T>(ObjectParameterKey<T> parameter, T value)
        {
            Set(GetAccessor(parameter), value);
        }

        /// <summary>
        /// Gets an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public T Get<T>(ObjectParameterKey<T> parameter)
        {
            return Get(GetAccessor(parameter));
        }

        /// <summary>
        /// Sets a permutation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public void Set<T>(PermutationParameterKey<T> parameter, T value)
        {
            Set(GetAccessor(parameter), value);
        }

        /// <summary>
        /// Gets a permutation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public T Get<T>(PermutationParameterKey<T> parameter)
        {
            return Get(GetAccessor(parameter));
        }

        /// <summary>
        /// Sets a blittable value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public void Set<T>(ValueParameterKey<T> parameter, T value) where T : struct
        {
            Set(GetAccessor(parameter), value);
        }

        /// <summary>
        /// Sets blittable values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="values"></param>
        public void Set<T>(ValueParameterKey<T> parameter, T[] values) where T : struct
        {
            Set(GetAccessor(parameter, values.Length), values);
        }

        /// <summary>
        /// Gets a blittable value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public T Get<T>(ValueParameterKey<T> parameter) where T : struct
        {
            return Get(GetAccessor(parameter));
        }

        /// <summary>
        /// Gets blittable values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T[] GetValues<T>(ValueParameterKey<T> key) where T : struct
        {
            var parameter = GetAccessor(key);
            var data = GetValuePointer(parameter);

            // Align to float4
            var stride = (Utilities.SizeOf<T>() + 15) / 16 * 16;
            var elementCount = (parameterKeyInfos[parameter.Index].Size + stride) / stride;
            var values = new T[elementCount];
            for (int i = 0; i < values.Length; ++i)
            {
                Utilities.Read(data, ref values[i]);
                data += stride;
            }

            return values;
        }

        /// <summary>
        /// Sets a blittable value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public void Set<T>(ValueParameter<T> parameter, T value) where T : struct
        {
            Utilities.Write(DataValues + parameterKeyInfos[parameter.Index].Offset, ref value);
        }

        /// <summary>
        /// Sets a blittable value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public void Set<T>(ValueParameter<T> parameter, ref T value) where T : struct
        {
            Utilities.Write(DataValues + parameterKeyInfos[parameter.Index].Offset, ref value);
        }

        /// <summary>
        /// Sets blittable values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="values"></param>
        public void Set<T>(ValueParameter<T> parameter, T[] values) where T : struct
        {
            var data = GetValuePointer(parameter);

            // Align to float4
            var stride = (Utilities.SizeOf<T>() + 15) / 16 * 16;
            var elementCount = (parameterKeyInfos[parameter.Index].Size + stride) / stride;
            if (values.Length > elementCount)
            {
                throw new IndexOutOfRangeException();
            }
            for (int i = 0; i < values.Length; ++i)
            {
                Utilities.Write(data, ref values[i]);
                data += stride;
            }
        }

        /// <summary>
        /// Sets a permutation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public void Set<T>(PermutationParameter<T> parameter, T value)
        {
            PermutationCounter++;
            ObjectValues[parameterKeyInfos[parameter.Index].BindingSlot] = value;
        }

        /// <summary>
        /// Sets an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterAccessor"></param>
        /// <param name="value"></param>
        public void Set<T>(ObjectParameterAccessor<T> parameterAccessor, T value)
        {
            ObjectValues[parameterKeyInfos[parameterAccessor.Index].BindingSlot] = value;
        }

        /// <summary>
        /// Gets a value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public T Get<T>(ValueParameter<T> parameter) where T : struct
        {
            return Utilities.Read<T>(DataValues + parameterKeyInfos[parameter.Index].Offset);
        }

        /// <summary>
        /// Gets a permutation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public T Get<T>(PermutationParameter<T> parameter)
        {
            return (T)ObjectValues[parameterKeyInfos[parameter.Index].BindingSlot];
        }

        /// <summary>
        /// Gets an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterAccessor"></param>
        /// <returns></returns>
        public T Get<T>(ObjectParameterAccessor<T> parameterAccessor)
        {
            return (T)ObjectValues[parameterKeyInfos[parameterAccessor.Index].BindingSlot];
        }

        public void Remove<T>(ParameterKey<T> key)
        {
            for (int i = 0; i < parameterKeyInfos.Count; ++i)
            {
                if (parameterKeyInfos[i].Key == key)
                {
                    parameterKeyInfos.SwapRemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Determines whether current collection contains a value for this key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(ParameterKey key)
        {
            for (int i = 0; i < parameterKeyInfos.Count; ++i)
            {
                if (parameterKeyInfos[i].Key == key)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reorganizes internal data and resources to match the given objects, and append extra values at the end.
        /// </summary>
        /// <param name="layoutParameterKeyInfos"></param>
        /// <param name="resourceCount"></param>
        /// <param name="bufferSize"></param>
        /// <param name="constantBuffers"></param>
        /// <param name="descriptorSetLayouts"></param>
        public void UpdateLayout(NextGenParameterCollectionLayout layout)
        {
            // Do a first pass to measure constant buffer size
            var newParameterKeyInfos = new FastList<ParameterKeyInfo>(Math.Max(1, parameterKeyInfos.Count));
            newParameterKeyInfos.AddRange(parameterKeyInfos);
            var processedParameters = new bool[parameterKeyInfos.Count];

            var bufferSize = layout.BufferSize;
            var resourceCount = layout.ResourceCount;

            // TODO: Should we perform a (read-only) copy?
            this.layoutParameterKeyInfos = layout.LayoutParameterKeyInfos;

            foreach (var layoutParameterKeyInfo in layoutParameterKeyInfos)
            {
                // Find the same parameter in old collection
                // Is this parameter already added?
                bool memberFound = false;
                for (int i = 0; i < parameterKeyInfos.Count; ++i)
                {
                    if (parameterKeyInfos[i].Key == layoutParameterKeyInfo.Key)
                    {
                        processedParameters[i] = true;
                        newParameterKeyInfos.Items[i] = layoutParameterKeyInfo;
                        break;
                    }
                }
            }

            // Append new elements that don't exist in new layouts (to preserve their values)
            for (int i = 0; i < processedParameters.Length; ++i)
            {
                // Skip parameters already processed before
                if (processedParameters[i])
                    continue;

                var parameterKeyInfo = newParameterKeyInfos[i];

                if (parameterKeyInfo.Offset != -1)
                {
                    // It's data
                    newParameterKeyInfos.Items[i].Offset = bufferSize;

                    bufferSize += newParameterKeyInfos.Items[i].Size;
                }
                else if (parameterKeyInfo.BindingSlot != -1)
                {
                    // It's a resource
                    newParameterKeyInfos.Items[i].BindingSlot = resourceCount++;
                }
            }
            
            var newDataValues = Marshal.AllocHGlobal(bufferSize);
            var newResourceValues = new object[resourceCount];

            // Update default values
            var bufferOffset = 0;
            foreach (var layoutParameterKeyInfo in layoutParameterKeyInfos)
            {
                if (layoutParameterKeyInfo.Offset != -1)
                {
                    // It's data
                    // TODO: Set default value
                    var defaultValueMetadata = layoutParameterKeyInfo.Key?.DefaultValueMetadata;
                    if (defaultValueMetadata != null)
                    {
                        defaultValueMetadata.WriteBuffer(newDataValues + bufferOffset + layoutParameterKeyInfo.Offset, 16);
                    }
                }
            }

            // Second pass to copy existing data at new offsets/slots
            for (int i = 0; i < parameterKeyInfos.Count; ++i)
            {
                var parameterKeyInfo = parameterKeyInfos[i];
                var newParameterKeyInfo = newParameterKeyInfos[i];

                if (newParameterKeyInfo.Offset != -1)
                {
                    // It's data
                    Utilities.CopyMemory(newDataValues + newParameterKeyInfo.Offset, DataValues + parameterKeyInfo.Offset, newParameterKeyInfo.Size);
                }
                else if (newParameterKeyInfo.BindingSlot != -1)
                {
                    // It's a resource
                    newResourceValues[newParameterKeyInfo.BindingSlot] = ObjectValues[parameterKeyInfo.BindingSlot];
                }
            }

            // Update new content
            parameterKeyInfos = newParameterKeyInfos;

            Marshal.FreeHGlobal(DataValues);
            DataValues = newDataValues;
            DataValuesSize = bufferSize;
            ObjectValues = newResourceValues;
        }

        private ObjectParameterAccessor<T> GetObjectParameterHelper<T>(ParameterKey<T> parameterKey, bool permutation)
        {
            // Find existing first
            for (int i = 0; i < parameterKeyInfos.Count; ++i)
            {
                if (parameterKeyInfos[i].Key == parameterKey)
                {
                    return new ObjectParameterAccessor<T>(i);
                }
            }

            if (permutation)
                PermutationCounter++;

            // Check layout if it exists
            if (layoutParameterKeyInfos.Count > 0)
            {
                foreach (var layoutParameterKeyInfo in layoutParameterKeyInfos)
                {
                    if (layoutParameterKeyInfo.Key == parameterKey)
                    {
                        parameterKeyInfos.Add(layoutParameterKeyInfo);
                        return new ObjectParameterAccessor<T>(parameterKeyInfos.Count - 1);
                    }
                }
            }

            // Create info entry
            var resourceValuesSize = ObjectValues?.Length ?? 0;
            Array.Resize(ref ObjectValues, resourceValuesSize + 1);
            parameterKeyInfos.Add(new ParameterKeyInfo(parameterKey, resourceValuesSize));

            // Initialize default value
            if (parameterKey.DefaultValueMetadata != null)
            {
                ObjectValues[resourceValuesSize] = parameterKey.DefaultValueMetadata.GetDefaultValue();
            }

            return new ObjectParameterAccessor<T>(parameterKeyInfos.Count - 1);
        }

        public class Serializer : ClassDataSerializer<NextGenParameterCollection>
        {
            public override void Serialize(ref NextGenParameterCollection parameterCollection, ArchiveMode mode, SerializationStream stream)
            {
                stream.Serialize(ref parameterCollection.parameterKeyInfos, mode);
                stream.SerializeExtended(ref parameterCollection.ObjectValues, mode);
                stream.Serialize(ref parameterCollection.DataValuesSize, mode);

                if (parameterCollection.DataValuesSize > 0)
                {
                    // If deserializing, allocate if necessary
                    if (mode == ArchiveMode.Deserialize)
                        parameterCollection.DataValues = Marshal.AllocHGlobal(parameterCollection.DataValuesSize);

                    stream.Serialize(parameterCollection.DataValues, parameterCollection.DataValuesSize);
                }
            }
        }
    }
}