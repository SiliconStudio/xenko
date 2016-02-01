// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Collections.Generic;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    internal class VertexArrayObjectInstance : IDisposable
    {
        private readonly VertexAttrib[] vertexAttribs;

        private readonly Dictionary<string, int> programAttributes;

        private readonly uint enabledVertexAttribArrays;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        private bool hasDynamicStagingVB;
#endif

        private int vaoId;

        private readonly GraphicsDevice graphicsDevice;

        private readonly int indexBufferId;

        public VertexArrayObjectInstance(GraphicsDevice graphicsDevice, EffectInputSignature effectInputSignature, VertexAttrib[] sharedVertexAttribs, int indexBufferId)
        {
            this.graphicsDevice = graphicsDevice;
            this.indexBufferId = indexBufferId;
            programAttributes = effectInputSignature.Attributes;

            int vertexAttributeCount = 0;
            for (int i = 0; i < sharedVertexAttribs.Length; i++)
            {
                if (programAttributes.ContainsKey(sharedVertexAttribs[i].AttributeName))
                {
                    vertexAttributeCount++;
                }
            }

            vertexAttribs = new VertexAttrib[vertexAttributeCount];

            int j = 0;
            for (int i = 0; i < sharedVertexAttribs.Length; i++)
            {
                AddAttribute(ref j, ref sharedVertexAttribs[i], ref enabledVertexAttribArrays);
            }
        }

        public void Dispose()
        {
            if (vaoId != 0)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (graphicsDevice.IsOpenGLES2)
                {
                    if (graphicsDevice.HasVAO)
                        OpenTK.Graphics.ES20.GL.Oes.DeleteVertexArrays(1, ref vaoId);
                }
                else
#endif
                {
                    GL.DeleteVertexArrays(1, ref vaoId);
                }

                vaoId = 0;
            }
        }

        private void AddAttribute(ref int index, ref VertexAttrib attrib, ref uint enabledVertexAttribArrays)
        {
            int attribIndex;
            if (programAttributes.TryGetValue(attrib.AttributeName, out attribIndex))
            {
                vertexAttribs[index] = attrib;
                vertexAttribs[index].Index = attribIndex;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                hasDynamicStagingVB |= attrib.VertexBufferId == 0;
#endif

                if (attribIndex != -1)
                    enabledVertexAttribArrays |= 1U << attribIndex;

                index++;
            }
        }

        /// <summary>
        /// Sets the VAO, creates if necessary
        /// </summary>
        internal void Apply(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice.HasVAO)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (hasDynamicStagingVB)
                {
                    if (graphicsDevice.IsOpenGLES2)
                        OpenTK.Graphics.ES20.GL.Oes.BindVertexArray(0);
                    else
                        GL.BindVertexArray(0);
                    ApplyAttributes(ref graphicsDevice.enabledVertexAttribArrays);
                }
                else
#endif
                    if (vaoId == 0)
                    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                        if (graphicsDevice.IsOpenGLES2)
                        {
                            OpenTK.Graphics.ES20.GL.Oes.GenVertexArrays(1, out vaoId);
                            OpenTK.Graphics.ES20.GL.Oes.BindVertexArray(vaoId);
                        }
                        else
#endif
                        {
                            GL.GenVertexArrays(1, out vaoId);
                            GL.BindVertexArray(vaoId);
                        }

                        // New VAO starts with no vertex attribs
                        uint currentlyEnabledVertexAttribArrays = 0;
                        ApplyAttributes(ref currentlyEnabledVertexAttribArrays);
                    }
                    else
                    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                        if (graphicsDevice.IsOpenGLES2)
                            OpenTK.Graphics.ES20.GL.Oes.BindVertexArray(vaoId);
                        else
#endif
                        {
                            GL.BindVertexArray(vaoId);
                        }

#if SILICONSTUDIO_PLATFORM_ANDROID
                        // Not sure why, but it seems PowerVR doesn't work well when changing VAO.
                        // This happened on Android 2.3, with a scene having both normal and skinned geometry (skinned geometry is trashed).
                        // Maybe this is related to changing both VAO and Program, something is not refreshed properly?
                        // TODO: Isolate case better: Only PowerVR SGX 540? Does it still happen in Android 4.0?
                        // Is it related to program changing or just a vertex attrib being added/removed compared to previous draw call?
                        if (graphicsDevice.Workaround_VAO_PowerVR_SGX_540)
                        {
                            // Disable unused vertex attribs (ones that are currently enabled and that should not be)
                            var vertexAttribsToReenable = enabledVertexAttribArrays;

                            int currentVertexAttribIndex = 0;
                            while (vertexAttribsToReenable != 0)
                            {
                                if ((vertexAttribsToReenable & 1) == 1)
                                {
                                    GL.DisableVertexAttribArray(currentVertexAttribIndex);
                                    GL.EnableVertexAttribArray(currentVertexAttribIndex);
                                }

                                currentVertexAttribIndex++;
                                vertexAttribsToReenable >>= 1;
                            }
                        }
#endif
                    }
            }
            else
            {
                ApplyAttributes(ref graphicsDevice.enabledVertexAttribArrays);
            }
        }

        private void ApplyAttributes(ref uint currentlyEnabledVertexAttribArrays)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);

            // Disable unused vertex attribs (ones that are currently enabled and that should not be)
            var vertexAttribsToDisable = currentlyEnabledVertexAttribArrays & ~enabledVertexAttribArrays;

            int currentVertexAttribIndex = 0;
            while (vertexAttribsToDisable != 0)
            {
                if ((vertexAttribsToDisable & 1) == 1)
                {
                    GL.DisableVertexAttribArray(currentVertexAttribIndex);
                }

                currentVertexAttribIndex++;
                vertexAttribsToDisable >>= 1;
            }

            int vertexBuffer = -1;

            foreach (var vertexAttrib in vertexAttribs)
            {
                if (vertexAttrib.VertexBufferId != vertexBuffer)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vertexAttrib.VertexBufferId);
                    vertexBuffer = vertexAttrib.VertexBufferId;
                }
                var vertexAttribMask = 1U << vertexAttrib.Index;
                if ((currentlyEnabledVertexAttribArrays & vertexAttribMask) == 0)
                {
                    GL.EnableVertexAttribArray(vertexAttrib.Index);
                }

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (vertexAttrib.IsInteger && !vertexAttrib.Normalized)
                    GL.VertexAttribIPointer(vertexAttrib.Index, vertexAttrib.Size, (VertexAttribIntegerType)vertexAttrib.Type, vertexAttrib.Stride, vertexAttrib.Offset);
                else
#endif
                    GL.VertexAttribPointer(vertexAttrib.Index, vertexAttrib.Size, vertexAttrib.Type, vertexAttrib.Normalized, vertexAttrib.Stride, vertexAttrib.Offset);
            }

            currentlyEnabledVertexAttribArrays = enabledVertexAttribArrays;
        }
    }
}
#endif