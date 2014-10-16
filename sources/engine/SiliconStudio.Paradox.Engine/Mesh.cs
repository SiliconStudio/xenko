using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox
{
    public class Mesh
    {
        public static readonly ParameterKey<ShaderMixinSource> AlbedoMaterial = ParameterKeys.Resource<ShaderMixinSource>();
        public static readonly ParameterKey<ShaderMixinSource> Tessellation = ParameterKeys.Resource<ShaderMixinSource>();
        public static readonly ParameterKey<bool> NeedAlphaBlending = ParameterKeys.Value(false);

        public ParameterCollection EffectParameters { get; set; }
    }
}