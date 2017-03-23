using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    // Avoid heap allocations, by pooling semantic types lazily.
    internal static class CompositionExtension
    {
        private static readonly Dictionary<Type, IRenderTargetSemantic> semanticsPool = new Dictionary<Type, IRenderTargetSemantic>();

        // Recycle an instance when needed. It is possible as long as implementations respect the stateless contract.
        internal static IRenderTargetSemantic ScoopSemantic(Type semanticType)
        {
            if (semanticsPool.ContainsKey(semanticType))
                return semanticsPool[semanticType];
            IRenderTargetSemantic semantic = (IRenderTargetSemantic)Activator.CreateInstance(semanticType);
            semanticsPool[semanticType] = semantic;
            return semantic;
        }

        internal static void AddTargetTo<TSemantic>(this RenderTargetSetup composition)
        {
            composition.AddTarget(new RenderTarget()
            {
                Description = new RenderTargetDesc() { Semantic = ScoopSemantic(typeof(TSemantic)) }
            });
        }
    }

    public partial class ForwardRenderer
    {
        /// <summary>
        /// Prepare the targets composition to allow effect permutations to be mixed in with the desired MRT output colorX class computers.
        /// </summary>
        protected virtual void CollectOpaqueStageRenderTargetComposition(RenderContext context)
        {
            TargetsComposition.Clear();

            TargetsComposition.AddTargetTo<ColorTargetSemantic>();

            bool normalsNeeded = PostEffects != null && PostEffects.RequiresNormalBuffer;
            if (normalsNeeded)
                TargetsComposition.AddTargetTo<NormalTargetSemantic>();

            bool velocityNeeded = PostEffects != null && PostEffects.RequiresVelocityBuffer;
            if (velocityNeeded)
                TargetsComposition.AddTargetTo<VelocityTargetSemantic>();

            bool rlrGBufferNeeded = PostEffects != null && PostEffects.RequiresSsrGBuffers;
            if (rlrGBufferNeeded)
            {
                TargetsComposition.AddTargetTo<OctaNormalSpecColorTargetSemantic>();
                TargetsComposition.AddTargetTo<EnvlightRoughnessTargetSemantic>();
            }
        }

        protected virtual void PrepareRenderTargetCreateParams(RenderDrawContext drawContext, Texture currentRenderTarget)
        {
            var colorParms = new RenderTargetTextureCreationParams()
            {
                PixelFormat = PostEffects != null ? PixelFormat.R16G16B16A16_Float : currentRenderTarget.ViewFormat,
                MSAALevel = actualMSAALevel,
                TextureFlags = TextureFlags.ShaderResource
            };
            TargetsComposition.SetTextureParams(typeof(ColorTargetSemantic), colorParms);

            var normalParams = new RenderTargetTextureCreationParams()
            {
                PixelFormat = PixelFormat.R16G16B16A16_Float,
                MSAALevel = actualMSAALevel,
                TextureFlags = TextureFlags.ShaderResource
            };
            TargetsComposition.SetTextureParams(typeof(NormalTargetSemantic), normalParams, SetPolicy.DefendSilentlyIfSemanticKeyNotFound);

            var velocityParams = new RenderTargetTextureCreationParams()
            {
                PixelFormat = PixelFormat.R16G16_Float,
                MSAALevel = actualMSAALevel,
                TextureFlags = TextureFlags.ShaderResource
            };
            TargetsComposition.SetTextureParams(typeof(VelocityTargetSemantic), velocityParams, SetPolicy.DefendSilentlyIfSemanticKeyNotFound);

            var rlrGBuffer1Params = new RenderTargetTextureCreationParams()
            {
                PixelFormat = PixelFormat.R16G16B16A16_Float,
                MSAALevel = actualMSAALevel,
                TextureFlags = TextureFlags.ShaderResource
            };
            TargetsComposition.SetTextureParams(typeof(OctaNormalSpecColorTargetSemantic), rlrGBuffer1Params, SetPolicy.DefendSilentlyIfSemanticKeyNotFound);

            var rlrGBuffer2Params = new RenderTargetTextureCreationParams()
            {
                PixelFormat = PixelFormat.R16G16B16A16_Float,
                MSAALevel = actualMSAALevel,
                TextureFlags = TextureFlags.ShaderResource
            };
            TargetsComposition.SetTextureParams(typeof(EnvlightRoughnessTargetSemantic), rlrGBuffer2Params, SetPolicy.DefendSilentlyIfSemanticKeyNotFound);
        }

        protected virtual Texture CreateRenderTargetTexture(RenderDrawContext drawContext, RenderTargetTextureCreationParams creationParams, int width, int height)
        {
            return PushScopedResource(
                drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                    TextureDescription.New2D(width, height, 1, creationParams.PixelFormat,
                        creationParams.TextureFlags | TextureFlags.RenderTarget, 1, GraphicsResourceUsage.Default,
                        creationParams.MSAALevel)));
        }

        protected virtual void CreateRegisteredRenderTargetTextures(RenderDrawContext drawContext, Texture currentRenderTarget)
        {
            foreach (var renderTarget in TargetsComposition.List)
            {
                bool canUseCurrent = renderTarget.Description.Semantic is ColorTargetSemantic
                                     && currentRenderTarget.Description.MultiSampleLevel == renderTarget.Description.RenderTargetTextureParams.MSAALevel
                                     && currentRenderTarget.Description.Format == renderTarget.Description.RenderTargetTextureParams.PixelFormat;
                var texture = canUseCurrent ? currentRenderTarget :
                    CreateRenderTargetTexture(drawContext, renderTarget.Description.RenderTargetTextureParams, currentRenderTarget.Width, currentRenderTarget.Height);
                TargetsComposition.SetTexture(renderTarget.Description.Semantic.GetType(), texture);
            }
        }

        /// <summary>
        /// Prepares targets per frame, caching and handling MSAA etc.
        /// </summary>
        /// <param name="drawContext">The current draw context</param>
        /// <param name="renderTargetsSize"></param>
        protected virtual void PrepareRenderTargetTextures(RenderDrawContext drawContext, Size2 renderTargetsSize)
        {
            var currentRenderTarget = drawContext.CommandList.RenderTarget;
            if (drawContext.CommandList.RenderTargetCount == 0)
                currentRenderTarget = null;
            var currentDepthStencil = drawContext.CommandList.DepthStencilBuffer;

            // Make sure we got a valid NOT MSAA final OUTPUT Target
            if (currentRenderTarget == null)
            {
                currentRenderTarget = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                    TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                        TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
            }

            PrepareRenderTargetCreateParams(drawContext, currentRenderTarget);

            CreateRegisteredRenderTargetTextures(drawContext, currentRenderTarget);

            //MSAA, we definitely need new buffers
            if (actualMSAALevel != MSAALevel.None)
            {
                //Handle Depth
                ViewDepthStencil = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                    TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, currentDepthStencil?.ViewFormat ?? PixelFormat.D24_UNorm_S8_UInt,
                        TextureFlags.ShaderResource | TextureFlags.DepthStencil, 1, GraphicsResourceUsage.Default, actualMSAALevel)));
            }
            else
            {
                //Handle Depth
                if (currentDepthStencil == null)
                {
                    currentDepthStencil = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                        TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.D24_UNorm_S8_UInt,
                            TextureFlags.ShaderResource | TextureFlags.DepthStencil)));
                }
                ViewDepthStencil = currentDepthStencil;
            }

            ViewOutputTarget = currentRenderTarget;
        }
    }
}
