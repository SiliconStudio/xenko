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
                if (parameterKeyInfos[i].Name == parameterKey.Name)
                {
                    return new ResourceParameter<T>(i);
                }
            }

            // Create info entry
            var resourceValuesSize = ResourceValues?.Length ?? 0;
            Array.Resize(ref ResourceValues, resourceValuesSize + 1);
            parameterKeyInfos.Add(new ParameterKeyInfo(parameterKey.Name, resourceValuesSize));
            return new ResourceParameter<T>(parameterKeyInfos.Count - 1);
        }

        public ValueParameter<T> GetValueParameter<T>(ParameterKey<T> parameterKey, int elementCount = 1) where T : struct
        {
            // Find existing first
            for (int i = 0; i < parameterKeyInfos.Count; ++i)
            {
                if (parameterKeyInfos[i].Name == parameterKey.Name)
                {
                    return new ValueParameter<T>(i);
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
            parameterKeyInfos.Add(new ParameterKeyInfo(parameterKey.Name, memberOffset, totalSize));

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
            foreach (var parameterKeyOffset in parameterKeyInfos)
            {
                if (parameterKeyOffset.Name == parameterKey.Name)
                {
                    return new ValueParameter<T>(parameterKeyOffset.Offset);
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
            parameterKeyInfos.Add(new ParameterKeyInfo(parameterKey.Name, memberOffset, totalSize));

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
        public void UpdateLayout(List<ShaderConstantBufferDescription> constantBuffers, List<DescriptorSetLayoutBuilder> descriptorSetLayouts)
        {
            // Do a first pass to measure constant buffer size
            var bufferSize = 0;
            var newParameterKeyInfos = new FastListStruct<ParameterKeyInfo>(Math.Max(1, parameterKeyInfos.Count));
            newParameterKeyInfos.AddRange(parameterKeyInfos);
            var processedParameters = new bool[parameterKeyInfos.Count];

            // Process constant buffers
            foreach (var constantBuffer in constantBuffers)
            {
                foreach (var member in constantBuffer.Members)
                {
                    // Is this parameter already added?
                    bool memberFound = false;
                    for (int i = 0; i < parameterKeyInfos.Count; ++i)
                    {
                        if (parameterKeyInfos[i].Name == member.Param.Key.Name)
                        {
                            memberFound = true;
                            processedParameters[i] = true;
                            newParameterKeyInfos.Items[i].Offset = bufferSize + member.Offset;
                            newParameterKeyInfos.Items[i].Size = member.Size;
                            break;
                        }
                    }

                    if (!memberFound)
                    {
                        // New item, let's add it
                        newParameterKeyInfos.Add(new ParameterKeyInfo(member.Param.Key.Name, bufferSize + member.Offset, member.Size));
                    }
                }

                bufferSize += constantBuffer.Size;
            }

            // Update or add resource bindings
            var currentBindingSlot = 0;
            for (int layoutIndex = 0; layoutIndex < descriptorSetLayouts.Count; layoutIndex++)
            {
                var layout = descriptorSetLayouts[layoutIndex];
                for (int entryIndex = 0; entryIndex < layout.Entries.Count; ++entryIndex, ++currentBindingSlot)
                {
                    // Is this parameter already added?
                    bool memberFound = false;
                    for (int i = 0; i < parameterKeyInfos.Count; ++i)
                    {
                        if (parameterKeyInfos[i].Name == layout.Entries[entryIndex].Name)
                        {
                            memberFound = true;
                            processedParameters[i] = true;
                            newParameterKeyInfos.Items[i].BindingSlot = currentBindingSlot;
                            break;
                        }
                    }

                    if (!memberFound)
                    {
                        // New item, let's add it
                        newParameterKeyInfos.Add(new ParameterKeyInfo(layout.Entries[entryIndex].Name, currentBindingSlot));
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
                    newParameterKeyInfos.Items[i].BindingSlot = currentBindingSlot++;
                }
            }
            
            var newDataValues = Marshal.AllocHGlobal(bufferSize);
            var newResourceValues = new object[currentBindingSlot];

            // Update default values
            var bufferOffset = 0;
            foreach (var constantBuffer in constantBuffers)
            {
                foreach (var member in constantBuffer.Members)
                {
                    var defaultValueMetadata = member.Param.Key?.DefaultValueMetadata;
                    if (defaultValueMetadata != null)
                    {
                        defaultValueMetadata.WriteBuffer(newDataValues + bufferOffset + member.Offset, 16);
                    }
                }
                bufferOffset += constantBuffer.Size;
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

        struct ParameterKeyInfo
        {
            // Common
            public string Name;

            // Values
            public int Offset;
            public int Size;

            // Resources
            public int BindingSlot;

            /// <summary>
            /// Describes a value parameter.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="offset"></param>
            /// <param name="size"></param>
            public ParameterKeyInfo(string name, int offset, int size)
            {
                Name = name;
                Offset = offset;
                Size = size;
                BindingSlot = -1;
            }

            /// <summary>
            /// Describes a resource parameter.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="bindingSlot"></param>
            public ParameterKeyInfo(string name, int bindingSlot)
            {
                Name = name;
                BindingSlot = bindingSlot;
                Offset = -1;
                Size = 1;
            }
        }
    }
}