using System;

namespace SiliconStudio.Paradox.Rendering
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