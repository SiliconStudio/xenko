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
        public object[] ResourceValues;

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

        public ResourceParameter<T> GetResourceParameter<T>(ParameterKey<T> parameterKey) where T : class
        {
            // Find existing first
            for (int i = 0; i < parameterKeyInfos.Count; ++i)
            {
                if (parameterKeyInfos[i].Key == parameterKey)
                {
                    return new ResourceParameter<T>(i);
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
                        return new ResourceParameter<T>(parameterKeyInfos.Count - 1);
                    }
                }
            }

            // Create info entry
            var resourceValuesSize = ResourceValues?.Length ?? 0;
            Array.Resize(ref ResourceValues, resourceValuesSize + 1);
            parameterKeyInfos.Add(new ParameterKeyInfo(parameterKey, resourceValuesSize));
            return new ResourceParameter<T>(parameterKeyInfos.Count - 1);
        }

        public ValueParameter<T> GetValueParameter<T>(ParameterKey<T> parameterKey, int elementCount = 1) where T : struct
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
        public ValueParameter<T> GetValueParameterArray<T>(ParameterKey<T[]> parameterKey, int elementCount = 1) where T : struct
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

        public IntPtr GetValuePointer<T>(ValueParameter<T> parameter) where T : struct
        {
            return DataValues + parameterKeyInfos[parameter.Index].Offset;
        }

        public void SetResourceSlow<T>(ParameterKey<T> parameter, T value) where T : class
        {
            Set(GetResourceParameter(parameter), value);
        }

        public T GetResourceSlow<T>(ParameterKey<T> parameter) where T : class
        {
            return Get(GetResourceParameter(parameter));
        }

        public void SetValueSlow<T>(ParameterKey<T> parameter, T value) where T : struct
        {
            Set(GetValueParameter(parameter), value);
        }

        public void SetValueSlow<T>(ParameterKey<T> parameter, T[] values) where T : struct
        {
            Set(GetValueParameter(parameter, values.Length), values);
        }

        public T GetValueSlow<T>(ParameterKey<T> parameter) where T : struct
        {
            return Get(GetValueParameter(parameter));
        }

        public T[] GetValuesSlow<T>(ParameterKey<T> key) where T : struct
        {
            var parameter = GetValueParameter(key);
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

        public void Set<T>(ValueParameter<T> parameter, T value) where T : struct
        {
            Utilities.Write(DataValues + parameterKeyInfos[parameter.Index].Offset, ref value);
        }

        public void Set<T>(ValueParameter<T> parameter, ref T value) where T : struct
        {
            Utilities.Write(DataValues + parameterKeyInfos[parameter.Index].Offset, ref value);
        }

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

        public void Set<T>(ResourceParameter<T> parameter, T value) where T : class
        {
            ResourceValues[parameterKeyInfos[parameter.Index].BindingSlot] = value;
        }

        public T Get<T>(ValueParameter<T> parameter) where T : struct
        {
            return Utilities.Read<T>(DataValues + parameterKeyInfos[parameter.Index].Offset);
        }

        public T Get<T>(ResourceParameter<T> parameter) where T : class
        {
            return (T)ResourceValues[parameterKeyInfos[parameter.Index].BindingSlot];
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
                    newResourceValues[newParameterKeyInfo.BindingSlot] = ResourceValues[parameterKeyInfo.BindingSlot];
                }
            }

            // Update new content
            parameterKeyInfos = newParameterKeyInfos;

            Marshal.FreeHGlobal(DataValues);
            DataValues = newDataValues;
            DataValuesSize = bufferSize;
            ResourceValues = newResourceValues;
        }

        public class Serializer : ClassDataSerializer<NextGenParameterCollection>
        {
            public override void Serialize(ref NextGenParameterCollection parameterCollection, ArchiveMode mode, SerializationStream stream)
            {
                stream.Serialize(ref parameterCollection.parameterKeyInfos, mode);
                stream.Serialize(ref parameterCollection.ResourceValues, mode);
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