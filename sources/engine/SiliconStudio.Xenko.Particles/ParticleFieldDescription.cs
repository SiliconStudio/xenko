// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Particles
{
    public class ParticleFieldDescription
    {
        private readonly int hashCode;
        public override int GetHashCode() => hashCode;

        private readonly string name;
        public string Name => name;

        protected ParticleFieldDescription(string name)
        {
            this.name = name;
            hashCode = name.GetHashCode();
        }
    }

    public class ParticleFieldDescription<T> : ParticleFieldDescription
    {
        private readonly T defaultValue;
        public T DefaultValue => defaultValue;

        public ParticleFieldDescription(string name) : base(name)
        {
        }

        public ParticleFieldDescription(string name, T defaultValue) : this(name)
        {
            this.defaultValue = defaultValue;
        }
    }
}
