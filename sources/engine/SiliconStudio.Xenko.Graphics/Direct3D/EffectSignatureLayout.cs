// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
using System.Linq;
using SharpDX.Direct3D11;

namespace SiliconStudio.Paradox.Graphics
{
    class EffectSignatureLayout
    {
        public InputElement[] InputElements { get; private set; }

        public byte[] ShaderSignature { get; private set; }

        private readonly int inputElementsHashCode = 0;

        public EffectSignatureLayout(InputElement[] inputElements, byte[] signature)
        {
            InputElements = inputElements;
            ShaderSignature = signature;
            inputElementsHashCode = InputElements.Aggregate(InputElements.Length, (current, inputElement) => (current * 397) ^ inputElement.GetHashCode());
            inputElementsHashCode = (inputElementsHashCode * 397) ^ ShaderSignature.GetHashCode();
        }

        public bool Equals(EffectSignatureLayout other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // Check the number of elements
            if (InputElements.Length != other.InputElements.Length)
                return false;

            // Check the signature pointer
            if (ShaderSignature != other.ShaderSignature)
                return false;

            return !InputElements.Where((t, i) => t != other.InputElements[i]).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(EffectSignatureLayout)) return false;
            return Equals((EffectSignatureLayout)obj);
        }

        public override int GetHashCode()
        {
            return inputElementsHashCode;
        }

        public static bool operator ==(EffectSignatureLayout left, EffectSignatureLayout right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EffectSignatureLayout left, EffectSignatureLayout right)
        {
            return !Equals(left, right);
        }
    }
} 
#endif 
