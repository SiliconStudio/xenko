using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Manage several effect parameters (resources and data). A specific data and resource layout can be forced (usually by the consuming effect).
    /// </summary>
    public class EffectInstanceParameters : IDisposable
    {
        private FastListStruct<ParameterKeyInfo> layoutParameterKeyInfos;
        private FastListStruct<ParameterKeyInfo> parameterKeyInfos = new FastListStruct<ParameterKeyInfo>(4);

        // Constants and resources
        // TODO: Currently stored in unmanaged array so we can get a pointer that can be updated from outside
        //   However, maybe ref locals would make this not needed anymore?
        internal IntPtr DataValues;
        internal int DataValuesSize;
        internal object[] ResourceValues;

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

        public void SetValue<T>(ValueParameter<T> parameter, T value) where T : struct
        {
            Utilities.Write(DataValues + parameterKeyInfos[parameter.Index].Offset, ref value);
        }

        public void SetValue<T>(ValueParameter<T> parameter, ref T value) where T : struct
        {
            Utilities.Write(DataValues + parameterKeyInfos[parameter.Index].Offset, ref value);
        }

        public void SetValues<T>(ValueParameter<T> parameter, T[] values) where T : struct
        {
            var data = GetValuePointer(parameter);

            // Align to float4
            var stride = (Utilities.SizeOf<T>() + 15) / 16 * 16;
            for (int i = 0; i < values.Length; ++i)
            {
                Utilities.Write(data, ref values[i]);
                data += stride;
            }
        }

        public void SetValue<T>(ResourceParameter<T> parameter, T value) where T : class
        {
            ResourceValues[parameterKeyInfos[parameter.Index].BindingSlot] = value;
        }

        /// <summary>
        /// Reorganizes internal data and resources to match the given objects, and append extra values at the end.
        /// </summary>
        /// <param name="constantBuffers"></param>
        /// <param name="descriptorSetLayouts"></param>
        public void UpdateLayout(FastListStruct<ParameterKeyInfo> layoutParameterKeyInfos, int bufferSize, int resourceCount)
        {
            // Do a first pass to measure constant buffer size
            var newParameterKeyInfos = new FastListStruct<ParameterKeyInfo>(Math.Max(1, parameterKeyInfos.Count));
            newParameterKeyInfos.AddRange(parameterKeyInfos);
            var processedParameters = new bool[parameterKeyInfos.Count];

            this.layoutParameterKeyInfos = layoutParameterKeyInfos;

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
            ResourceValues = newResourceValues;
        }

        public struct ParameterKeyInfo
        {
            // Common
            public ParameterKey Key;

            // Values
            public int Offset;
            public int Size;

            // Resources
            public int BindingSlot;

            /// <summary>
            /// Describes a value parameter.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="offset"></param>
            /// <param name="size"></param>
            public ParameterKeyInfo(ParameterKey key, int offset, int size)
            {
                Key = key;
                Offset = offset;
                Size = size;
                BindingSlot = -1;
            }

            /// <summary>
            /// Describes a resource parameter.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="bindingSlot"></param>
            public ParameterKeyInfo(ParameterKey key, int bindingSlot)
            {
                Key = key;
                BindingSlot = bindingSlot;
                Offset = -1;
                Size = 1;
            }
        }
    }
}