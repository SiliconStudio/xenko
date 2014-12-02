// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL 
using System;
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using ES30 = OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Paradox.Graphics
{
    public static class OpenGLConvertExtensions
    {
        public static ErrorCode GetErrorCode()
        {
#if SILICONSTUDIO_PLATFORM_ANDROID || SILICONSTUDIO_PLATFORM_IOS
            return GL.GetErrorCode();
#else
            return GL.GetError();
#endif
        }

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES && !(SILICONSTUDIO_PLATFORM_ANDROID || SILICONSTUDIO_PLATFORM_IOS)
        public static TextureComponentCount ToOpenGL(this PixelInternalFormat format)
        {
            switch (format)
            {
                case PixelInternalFormat.Alpha:
                    return TextureComponentCount.Alpha;
                case PixelInternalFormat.Rgb:
                    return TextureComponentCount.Rgb;
                case PixelInternalFormat.Rgba:
                    return TextureComponentCount.Rgba;
                case PixelInternalFormat.Luminance:
                    return TextureComponentCount.Luminance;
                case PixelInternalFormat.LuminanceAlpha:
                    return TextureComponentCount.LuminanceAlpha;
                default:
                    throw new ArgumentOutOfRangeException("format");
            }
        }

        public static TextureTarget2d ToOpenGL(this TextureTarget target)
        {
            switch (target)
            {
                case TextureTarget.Texture2D:
                    return TextureTarget2d.Texture2D;
                case TextureTarget.TextureCubeMapPositiveX:
                    return TextureTarget2d.TextureCubeMapPositiveX;
                case TextureTarget.TextureCubeMapNegativeX:
                    return TextureTarget2d.TextureCubeMapNegativeX;
                case TextureTarget.TextureCubeMapPositiveY:
                    return TextureTarget2d.TextureCubeMapPositiveY;
                case TextureTarget.TextureCubeMapNegativeY:
                    return TextureTarget2d.TextureCubeMapNegativeY;
                case TextureTarget.TextureCubeMapPositiveZ:
                    return TextureTarget2d.TextureCubeMapPositiveZ;
                case TextureTarget.TextureCubeMapNegativeZ:
                    return TextureTarget2d.TextureCubeMapNegativeZ;
                default:
                    throw new NotImplementedException();
            }
        }

        public static ES30.PrimitiveType ToOpenGL(this PrimitiveType primitiveType)
        {
            return primitiveType.ToOpenGLES();
        }
#else
        public static PixelInternalFormat ToOpenGL(this PixelInternalFormat format)
        {
            return format;
        }

        public static TextureTarget ToOpenGL(this TextureTarget target)
        {
            return target;
        }

        public static BeginMode ToOpenGL(this PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.PointList:
                    return BeginMode.Points;
                case PrimitiveType.LineList:
                    return BeginMode.Lines;
                case PrimitiveType.LineStrip:
                    return BeginMode.LineStrip;
                case PrimitiveType.TriangleList:
                    return BeginMode.Triangles;
                case PrimitiveType.TriangleStrip:
                    return BeginMode.TriangleStrip;
                default:
                    throw new NotImplementedException();
            }
        }
#endif

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
        public static ES30.PrimitiveType ToOpenGLES(this PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.PointList:
                    return ES30.PrimitiveType.Points;
                case PrimitiveType.LineList:
                    return ES30.PrimitiveType.Lines;
                case PrimitiveType.LineStrip:
                    return ES30.PrimitiveType.LineStrip;
                case PrimitiveType.TriangleList:
                    return ES30.PrimitiveType.Triangles;
                case PrimitiveType.TriangleStrip:
                    return ES30.PrimitiveType.TriangleStrip;
                default:
                    throw new NotImplementedException();
            }
        }
#endif

        public static TextureWrapMode ToOpenGL(this TextureAddressMode addressMode)
        {
            switch (addressMode)
            {
#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                case TextureAddressMode.Border:
                    return TextureWrapMode.ClampToBorder;
#endif
                case TextureAddressMode.Clamp:
                    return TextureWrapMode.ClampToEdge;
                case TextureAddressMode.Mirror:
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES && !(SILICONSTUDIO_PLATFORM_ANDROID || SILICONSTUDIO_PLATFORM_IOS)
                    return TextureWrapMode.Repeat;
#else
                    return TextureWrapMode.MirroredRepeat;
#endif
                case TextureAddressMode.Wrap:
                    return TextureWrapMode.Repeat;
                default:
                    throw new NotImplementedException();
            }
        }

        public static DepthFunction ToOpenGLDepthFunction(this CompareFunction function)
        {
            switch (function)
            {
                case CompareFunction.Always:
                    return DepthFunction.Always;
                case CompareFunction.Equal:
                    return DepthFunction.Equal;
                case CompareFunction.GreaterEqual:
                    return DepthFunction.Gequal;
                case CompareFunction.Greater:
                    return DepthFunction.Greater;
                case CompareFunction.LessEqual:
                    return DepthFunction.Lequal;
                case CompareFunction.Less:
                    return DepthFunction.Less;
                case CompareFunction.Never:
                    return DepthFunction.Never;
                case CompareFunction.NotEqual:
                    return DepthFunction.Notequal;
                default:
                    throw new NotImplementedException();
            }
        }
        
        public static StencilFunction ToOpenGLStencilFunction(this CompareFunction function)
        {
            switch (function)
            {
                case CompareFunction.Always:
                    return StencilFunction.Always;
                case CompareFunction.Equal:
                    return StencilFunction.Equal;
                case CompareFunction.GreaterEqual:
                    return StencilFunction.Gequal;
                case CompareFunction.Greater:
                    return StencilFunction.Greater;
                case CompareFunction.LessEqual:
                    return StencilFunction.Lequal;
                case CompareFunction.Less:
                    return StencilFunction.Less;
                case CompareFunction.Never:
                    return StencilFunction.Never;
                case CompareFunction.NotEqual:
                    return StencilFunction.Notequal;
                default:
                    throw new NotImplementedException();
            }
        }

        public static StencilOp ToOpenGL(this StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Keep:
                    return StencilOp.Keep;
                case StencilOperation.Zero:
                    return StencilOp.Zero;
                case StencilOperation.Replace:
                    return StencilOp.Replace;
                case StencilOperation.IncrementSaturation:
                    return StencilOp.Incr;
                case StencilOperation.DecrementSaturation:
                    return StencilOp.Decr;
                case StencilOperation.Invert:
                    return StencilOp.Invert;
                case StencilOperation.Increment:
                    return StencilOp.IncrWrap;
                case StencilOperation.Decrement:
                    return StencilOp.DecrWrap;
                default:
                    throw new ArgumentOutOfRangeException("operation");
            }
        }
    }
}
 
#endif 
