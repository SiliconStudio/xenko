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

        public Effect Effect;
        public RenderEffectReflection Reflection;

        public VertexArrayObject VertexArrayObject;
        public DescriptorSet[] DescriptorSets;

        /// <summary>
        /// Validates if effect needs to be compiled or recompiled.
        /// </summary>
        public EffectValidator EffectValidator;

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
    }


    /// <summary>
    /// Describes an effect as used by a <see cref="RenderNode"/>.
    /// </summary>
    public class RenderEffectReflection
    {
        public FrameResourceGroupLayout PerFrameLayout;
        public ViewResourceGroupLayout PerViewLayout;
        public RenderSystemResourceGroupLayout PerDrawLayout;

        // PerFrame
        public ResourceGroup PerFrameResources;

        // TODO: Should be stored in a per-effect property
        public ResourceGroupLayout PerLightingLayout;

        public EffectBinder Binder;
    }
}