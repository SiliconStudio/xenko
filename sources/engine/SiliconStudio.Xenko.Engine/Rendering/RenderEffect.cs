using System.Threading.Tasks;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Instantiation of an Effect for a given <see cref="StaticEffectObjectNodeReference"/>.
    /// </summary>
    public class RenderEffect
    {
        // Request effect name
        public readonly string EffectName;

        public int LastFrameUsed { get; private set; }

        /// <summary>
        /// Describes what state the effect is in (compiling, error, etc..)
        /// </summary>
        public RenderEffectState State;

        public Effect Effect;
        public RenderEffectReflection Reflection;

        /// <summary>
        /// Compiled pipeline state.
        /// </summary>
        public PipelineState PipelineState;

        /// <summary>
        /// Validates if effect needs to be compiled or recompiled.
        /// </summary>
        public EffectValidator EffectValidator;

        /// <summary>
        /// Pending effect being compiled.
        /// </summary>
        public Task<Effect> PendingEffect;

        public EffectParameterUpdater FallbackParameterUpdater;
        public NextGenParameterCollection FallbackParameters;

        public RenderEffect(string effectName)
        {
            EffectName = effectName;
            EffectValidator.Initialize();
        }

        /// <summary>
        /// Mark effect as used during this frame.
        /// </summary>
        /// <returns>True if state changed (object was not mark as used during this frame until now), otherwise false.</returns>
        public bool MarkAsUsed(NextGenRenderSystem renderSystem)
        {
            if (LastFrameUsed == renderSystem.FrameCounter)
                return false;

            LastFrameUsed = renderSystem.FrameCounter;
            return true;
        }

        public bool IsUsedDuringThisFrame(NextGenRenderSystem renderSystem)
        {
            return LastFrameUsed == renderSystem.FrameCounter;
        }

        public void ClearFallbackParameters()
        {
            FallbackParameterUpdater = default(EffectParameterUpdater);
            FallbackParameters = null;
        }
    }


    /// <summary>
    /// Describes an effect as used by a <see cref="RenderNode"/>.
    /// </summary>
    public class RenderEffectReflection
    {
        public RootSignature RootSignature;

        public FrameResourceGroupLayout PerFrameLayout;
        public ViewResourceGroupLayout PerViewLayout;
        public RenderSystemResourceGroupLayout PerDrawLayout;

        // PerFrame
        public ResourceGroup PerFrameResources;

        public ResourceGroupBufferUploader BufferUploader;

        public EffectDescriptorSetReflection DescriptorReflection;

        // Used only for fallback effect
        public EffectParameterUpdaterLayout FallbackUpdaterLayout;
        public int[] FallbackResourceGroupMapping;
    }
}