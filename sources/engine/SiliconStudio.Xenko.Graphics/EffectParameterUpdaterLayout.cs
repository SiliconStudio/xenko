using System.Linq;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines a layout used by <see cref="EffectParameterUpdater"/> to update several <see cref="ResourceGroup"/> from a <see cref="ParameterCollection"/>.
    /// </summary>
    public class EffectParameterUpdaterLayout
    {
        internal ResourceGroupLayout[] ResourceGroupLayouts;

        internal DescriptorSetLayoutBuilder[] Layouts;
        internal ParameterCollectionLayout ParameterCollectionLayout = new ParameterCollectionLayout();

        public EffectParameterUpdaterLayout(GraphicsDevice graphicsDevice, Effect effect, DescriptorSetLayoutBuilder[] layouts)
        {
            Layouts = layouts;

            // Process constant buffers
            ResourceGroupLayouts = new ResourceGroupLayout[layouts.Length];
            for (int layoutIndex = 0; layoutIndex < layouts.Length; layoutIndex++)
            {
                var layout = layouts[layoutIndex];

                ParameterCollectionLayout.ProcessResources(layout);

                string cbufferName = null;

                for (int entryIndex = 0; entryIndex < layout.Entries.Count; ++entryIndex)
                {
                    var layoutEntry = layout.Entries[entryIndex];
                    if (layoutEntry.Class == EffectParameterClass.ConstantBuffer)
                    {
                        var constantBuffer = effect.Bytecode.Reflection.ConstantBuffers.First(x => x.Name == layoutEntry.Key.Name);
                        ParameterCollectionLayout.ProcessConstantBuffer(constantBuffer);

                        // For now we assume first cbuffer will be the main one
                        if (cbufferName == null)
                            cbufferName = layoutEntry.Key.Name;
                    }
                }

                ResourceGroupLayouts[layoutIndex] = ResourceGroupLayout.New(graphicsDevice, layout, effect.Bytecode, cbufferName);
            }
        }
    }
}