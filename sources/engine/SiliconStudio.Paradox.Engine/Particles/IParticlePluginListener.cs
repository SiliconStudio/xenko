// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Particles
{
    /// <summary>
    /// Listeners that can react on addition or removal to a <see cref="ParticleSystem"/>.
    /// </summary>
    public interface IParticlePluginListener : IParticlePlugin
    {
        void OnAddPlugin(ParticleSystem particleSystem);
        void OnRemovePlugin(ParticleSystem particleSystem);
    }
}