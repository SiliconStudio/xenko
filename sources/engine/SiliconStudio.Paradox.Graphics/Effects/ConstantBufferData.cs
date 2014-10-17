// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Graphics.Internals
{
    internal class ConstantBufferData
    {
        public IntPtr Data { get; private set; }
        public ShaderConstantBufferDescription Desc { get; private set; }

        private BoundConstantBufferParam[] constantBufferParams;

        public ConstantBufferData(ShaderConstantBufferDescription description)
        {
            Desc = description;
            Data = Marshal.AllocHGlobal(Desc.Size);
            constantBufferParams = new BoundConstantBufferParam[Desc.Members.Length];
        }

        public bool Update(ShaderParameterUpdater parameterUpdater)
        {
            bool dataChanged = false;

            //fixed (BoundConstantBufferParam* paramReferences = &this.constantBufferParams[0])
            {
                for (int i = 0; i < this.constantBufferParams.Length; ++i)
                {
                    dataChanged |= UpdateValue(parameterUpdater, ref Desc.Members[i], i);
                }
            }
            return dataChanged;
        }

        private unsafe bool UpdateValue(ShaderParameterUpdater parameterUpdater, ref EffectParameterValueData shaderVariable, int i)
        {
            if (shaderVariable.Param.KeyIndex == -1)
            {
                throw new InvalidOperationException();
            }

            BoundConstantBufferParam paramReference = constantBufferParams[i];

            var internalValue = parameterUpdater.GetInternalValue(shaderVariable.Param.KeyIndex);

            // TODO: Comparing Counter+DataPointer is not enough (if realloc on same address)
            if (internalValue.Counter == paramReference.DirtyCount
                && internalValue == paramReference.DataPointer)
                return false;

            constantBufferParams[i] = new BoundConstantBufferParam
            {
                DataPointer = internalValue,
                DirtyCount = internalValue.Counter
            };

            var destination = (byte*)(Data + shaderVariable.Offset);

            int sourceOffset = 0;

            float* variableData = (float*)destination; // + shaderVariable.Offset);

            Matrix tempMatrix;

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

            return true;
        }

        private struct BoundConstantBufferParam
        {
            public int DirtyCount;
            public ParameterCollection.InternalValue DataPointer;
        }
    }
}