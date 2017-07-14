// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles
{
    public abstract class ParticleFieldDescription
    {
        private readonly int hashCode;

        protected ParticleFieldDescription(string name)
        {
            Name = name;
            hashCode = name?.GetHashCode() ?? 0;
            FieldSize = 0;
        }

        public override int GetHashCode() => hashCode;

        public int FieldSize { get; protected set; }

        public string Name { get; }
    }

    public class ParticleFieldDescription<T> : ParticleFieldDescription where T : struct
    {
        public ParticleFieldDescription(string name)
            : base(name)
        {
            FieldSize = ParticleUtilities.AlignedSize(Utilities.SizeOf<T>(), 4);
        }

        public ParticleFieldDescription(string name, T defaultValue)
            : this(name)
        {
            DefaultValue = defaultValue;
        }

        public T DefaultValue { get; }
    }
}
