// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Graphics.Internals
{
    internal class ConstantBufferData
    {
        public IntPtr Data { get; private set; }
        public ShaderConstantBufferDescription Desc { get; private set; }

        private ParameterCollectionGroup.BoundInternalValue[] constantBufferParams;

        public ConstantBufferData(ShaderConstantBufferDescription description)
        {
            Desc = description;
            Data = Marshal.AllocHGlobal(Desc.Size);
            constantBufferParams = new ParameterCollectionGroup.BoundInternalValue[Desc.Members.Length];
        }

        /// <summary>
        /// Updates the specified parameter updater.
        /// </summary>
        /// <param name="parameterCollectionGroup">The parameter updater.</param>
        /// <returns></returns>
        public unsafe bool Update(EffectParameterCollectionGroup parameterCollectionGroup)
        {
            bool dataChanged = false;
            Matrix tempMatrix;

            //fixed (BoundConstantBufferParam* paramReferences = &this.constantBufferParams[0])
            if (constantBufferParams.Length > 0)
            {
                var shaderVariable = Interop.Pin(ref Desc.Members[0]);
                for (int i = 0; i < this.constantBufferParams.Length; ++i, shaderVariable = Interop.IncrementPinned(shaderVariable))
                {
                    if (shaderVariable.Param.KeyIndex == -1)
                    {
                        throw new InvalidOperationException();
                    }

                    var paramReference = constantBufferParams[i];

                    var internalValue = parameterCollectionGroup.GetInternalValue(shaderVariable.Param.KeyIndex);

                    // TODO: Comparing Counter+DataPointer is not enough (if realloc on same address)
                    if (internalValue == paramReference.Value
                        && internalValue.Counter == paramReference.DirtyCount)
                        continue;

                    constantBufferParams[i] = new ParameterCollectionGroup.BoundInternalValue
                    {
                        Value = internalValue,
                        DirtyCount = internalValue.Counter
                    };

                    var destination = (byte*)(Data + shaderVariable.Offset);

                    int sourceOffset = 0;

                    float* variableData = (float*)destination; // + shaderVariable.Offset);

                    switch (shaderVariable.Param.Class)
                    {
                        case EffectParameterClass.Struct:
                            internalValue.ReadFrom((IntPtr)variableData, sourceOffset, shaderVariable.Size);
                            break;
                        case EffectParameterClass.Scalar:
                            for (int elt = 0; elt < shaderVariable.Count; ++elt)
                            {
                                internalValue.ReadFrom((IntPtr)variableData, sourceOffset, sizeof(float));
                                //*variableData = *source++;
                                sourceOffset += 4;
                                variableData += 4; // 4 floats
                            }
                            break;
                        case EffectParameterClass.Vector:
                        case EffectParameterClass.Color:
                            for (int elt = 0; elt < shaderVariable.Count; ++elt)
                            {
                                //Framework.Utilities.CopyMemory((IntPtr)variableData, (IntPtr)source, (int)(shaderVariable.ColumnCount * sizeof(float)));
                                internalValue.ReadFrom((IntPtr)variableData, sourceOffset, (int)(shaderVariable.ColumnCount * sizeof(float)));
                                sourceOffset += (int)shaderVariable.ColumnCount * 4;
                                variableData += 4;
                            }
                            break;
                        case EffectParameterClass.MatrixColumns:
                            for (int elt = 0; elt < shaderVariable.Count; ++elt)
                            {
                                //fixed (Matrix* p = &tempMatrix)
                                {
                                    internalValue.ReadFrom((IntPtr)(byte*)&tempMatrix, sourceOffset, (int)(shaderVariable.ColumnCount * shaderVariable.RowCount * sizeof(float)));
                                    ((Matrix*)variableData)->CopyMatrixFrom((float*)&tempMatrix, unchecked((int)shaderVariable.ColumnCount), unchecked((int)shaderVariable.RowCount));
                                    sourceOffset += (int)(shaderVariable.ColumnCount * shaderVariable.RowCount) * 4;
                                    variableData += 4 * shaderVariable.RowCount;
                                }
                            }
                            break;
                        case EffectParameterClass.MatrixRows:
                            for (int elt = 0; elt < shaderVariable.Count; ++elt)
                            {
                                //fixed (Matrix* p = &tempMatrix)
                                {
                                    internalValue.ReadFrom((IntPtr)(byte*)&tempMatrix, sourceOffset, (int)(shaderVariable.ColumnCount * shaderVariable.RowCount * sizeof(float)));
                                    ((Matrix*)variableData)->TransposeMatrixFrom((float*)&tempMatrix, unchecked((int)shaderVariable.ColumnCount), unchecked((int)shaderVariable.RowCount));
                                    //source += shaderVariable.ColumnCount * shaderVariable.RowCount;
                                    sourceOffset += (int)(shaderVariable.ColumnCount * shaderVariable.RowCount) * 4;
                                    variableData += 4 * shaderVariable.RowCount;
                                }
                            }
                            break;
                    }

                    dataChanged = true;
                }
            }
            return dataChanged;
        }
    }
}