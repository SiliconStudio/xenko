// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Particles
{
    /// <summary>
    /// Describes a field for a particle, which can store specific data for every particle.
    /// </summary>
    public class ParticleFieldDescription
    {
        private readonly int hashCode;
        private readonly string name;

        protected ParticleFieldDescription(string name)
        {
            this.hashCode = name.GetHashCode();
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }

    /// <summary>
    /// Describes a field for a particle, which can store specific data for every particle.
    /// </summary>
    public class ParticleFieldDescription<T> : ParticleFieldDescription
    {
        private readonly T defaultValue;

        public T DefaultValue
        {
            get { return defaultValue; }
        }

        public ParticleFieldDescription(string name)
            : base(name)
        {
        }

        public ParticleFieldDescription(string name, T defaultValue)
            : this(name)
        {
            this.defaultValue = defaultValue;
        }
    }
}