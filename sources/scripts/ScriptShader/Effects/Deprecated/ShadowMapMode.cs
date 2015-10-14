using System;

namespace SiliconStudio.Xenko.Rendering
{
    [Flags]
    public enum ShadowMapMode
    {
        None = 0,
        Caster = 1,
        Receiver = 2,
        All = Caster | Receiver,
    }
}