using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public class RenderSystemResourceGroupLayout : ResourceGroupLayout
    {
        internal int[] ConstantBufferOffsets;
        internal int[] ResourceIndices;

        public int GetConstantBufferOffset(ConstantBufferOffsetReference offsetReference)
        {
            return ConstantBufferOffsets[offsetReference.Index];
        }
    }

    public struct ResourceGroupEntry
    {
        public int LastFrameUsed;
        public ResourceGroup Resources;

        /// <summary>
        /// Mark resource group as used during this frame.
        /// </summary>
        /// <returns>True if state changed (object was not mark as used during this frame until now), otherwise false.</returns>
        public bool MarkAsUsed(NextGenRenderSystem renderSystem)
        {
            if (LastFrameUsed == renderSystem.FrameCounter)
                return false;

            LastFrameUsed = renderSystem.FrameCounter;
            return true;
        }
    }

    public class FrameResourceGroupLayout : RenderSystemResourceGroupLayout
    {
        public ResourceGroupEntry Entry;
    }

    public class ViewResourceGroupLayout : RenderSystemResourceGroupLayout
    {
        public ResourceGroupEntry[] Entries;
    }
}