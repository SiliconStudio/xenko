using System.Collections.Generic;
using SiliconStudio.Xenko.Graphics;

namespace RenderArchitecture
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


    public class EffectDescriptorSetReflection
    {
        internal List<LayoutEntry> Layouts { get; } = new List<LayoutEntry>();

        public DescriptorSetLayoutBuilder GetLayout(string name)
        {
            foreach (var entry in Layouts)
            {
                if (entry.Name == name)
                    return entry.Layout;
            }

            return null;
        }

        public int GetLayoutIndex(string name)
        {
            for (int index = 0; index < Layouts.Count; index++)
            {
                if (Layouts[index].Name == name)
                    return index;
            }

            return -1;
        }

        public void AddLayout(string descriptorSetName, DescriptorSetLayoutBuilder descriptorSetLayoutBuilder)
        {
            Layouts.Add(new LayoutEntry(descriptorSetName, descriptorSetLayoutBuilder));
        }

        internal struct LayoutEntry
        {
            public string Name;
            public DescriptorSetLayoutBuilder Layout;

            public LayoutEntry(string name, DescriptorSetLayoutBuilder layout)
            {
                Name = name;
                Layout = layout;
            }
        }
    }


    /// <summary>
    /// Describes an effect as used by a <see cref="RenderNode"/>.
    /// </summary>
    public class RenderEffectReflection
    {
        public FrameResourceGroupLayout PerFrameLayout;
        public ViewResourceGroupLayout PerViewLayout;
        public ResourceGroupLayout PerDrawLayout;

        // PerFrame
        public ResourceGroup PerFrameResources;

        // TODO: Should be stored in a per-effect property
        public ResourceGroupLayout PerLightingLayout;

        public EffectBinder Binder;
    }
}