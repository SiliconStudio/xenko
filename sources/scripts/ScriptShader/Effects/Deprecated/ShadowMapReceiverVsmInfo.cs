using System.Runtime.InteropServices;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct ShadowMapReceiverVsmInfo
    {
        public float BleedingFactor;
        public float MinVariance;
        public Vector2 Padding0;
    }
}